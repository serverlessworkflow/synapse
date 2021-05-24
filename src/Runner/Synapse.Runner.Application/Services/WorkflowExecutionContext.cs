using CloudNative.CloudEvents;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowExecutionContext"/> interface
    /// </summary>
    public class WorkflowExecutionContext
        : IWorkflowExecutionContext
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowExecutionContext"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="applicationLifetime">The service used to manage the application's lifetime</param>
        /// <param name="expressionEvaluatorFactory">The service used to create <see cref="IExpressionEvaluator"/>s</param>
        /// <param name="definitions">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="instances">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="triggers">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1Trigger"/>s</param>
        /// <param name="cloudEventFormatter">The service used to format <see cref="CloudEvent"/>s</param>
        public WorkflowExecutionContext(ILogger<WorkflowExecutionContext> logger, IHostApplicationLifetime applicationLifetime, IExpressionEvaluatorFactory expressionEvaluatorFactory, IRepository<V1Workflow> definitions, 
            IRepository<V1WorkflowInstance> instances, IRepository<V1Trigger> triggers, ICloudEventFormatter cloudEventFormatter)
        {
            this.Logger = logger;
            this.ApplicationLifetime = applicationLifetime;
            this.ExpressionEvaluatorFactory = expressionEvaluatorFactory;
            this.Definitions = definitions;
            this.Instances = instances;
            this.Triggers = triggers;
            this.CloudEventFormatter = cloudEventFormatter;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to manage the application's lifetime
        /// </summary>
        protected IHostApplicationLifetime ApplicationLifetime { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IExpressionEvaluator"/>s
        /// </summary>
        protected IExpressionEvaluatorFactory ExpressionEvaluatorFactory { get; }

        /// <summary>
        /// Gets the service used to format <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventFormatter CloudEventFormatter { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Definitions { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> Instances { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1Trigger"/>s
        /// </summary>
        protected IRepository<V1Trigger> Triggers { get; }

        /// <summary>
        /// Gets the <see cref="AsyncLock"/> used to ensure thread safe use of the <see cref="WorkflowExecutionContext"/>
        /// </summary>
        protected AsyncLock Lock { get; } = new AsyncLock();

        /// <inheritdoc/>
        public V1Workflow Definition { get; private set; }

        /// <inheritdoc/>
        public V1WorkflowInstance Instance { get; private set; }

        /// <inheritdoc/>
        public IExpressionEvaluator ExpressionEvaluator { get; private set; }

        /// <inheritdoc/>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation($"Initializing the current workflow context...");
                this.Definition = await this.Definitions.FindAsync(SynapseConstants.EnvironmentVariables.Workflows.Id.Value, SynapseConstants.EnvironmentVariables.Workflows.Version.Value, cancellationToken: cancellationToken);
                if (this.Definition == null)
                    throw new NullReferenceException($"Failed to find the Workflow Custom Resource '{SynapseConstants.EnvironmentVariables.Workflows.Id.Value}:{SynapseConstants.EnvironmentVariables.Workflows.Version.Value}'. The runner might be incorrectly configured.");
                string instanceId = SynapseConstants.EnvironmentVariables.Workflows.Instance.Value;
                if (string.IsNullOrWhiteSpace(instanceId))
                {
                    if (SynapseConstants.EnvironmentVariables.Runtime.Startup.Value == EnumHelper.Stringify(RuntimeStartupType.Schedule))
                    {
                        this.Instance = new V1WorkflowInstance()
                        {
                            Metadata = new()
                            {
                                GenerateName = $"{this.Definition.Spec.Definition.Id}-".ToLower(),
                                NamespaceProperty = SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value,
                                Labels = new Dictionary<string, string>() { { SynapseConstants.Labels.Scheduled, true.ToString() } }
                            },
                            Spec = new()
                            {
                                Definition = new V1WorkflowReference(SynapseConstants.EnvironmentVariables.Workflows.Id.Value, SynapseConstants.EnvironmentVariables.Workflows.Version.Value),
                                Input = new JObject()
                            }
                        };
                        this.Instance = await this.Instances.AddAsync(this.Instance, cancellationToken);
                        this.Instance.Initialize();
                        this.Instance = await this.Instances.AddAsync(this.Instance, cancellationToken);
                        this.Instance.Label();
                        this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                        this.Instance.Deploy(SynapseConstants.EnvironmentVariables.Kubernetes.PodName.Value);
                        this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                    }
                    else
                    {
                        throw new NullReferenceException($"The Workflow Instance Custom Resource id has not been set. The runner might be incorrectly configured.");
                    }
                }
                else
                {
                    this.Instance = await Instances.FindByNameAsync(instanceId, cancellationToken);
                }
                if (this.Instance == null)
                    throw new NullReferenceException($"Failed to find the Workflow Instance Custom Resource '{SynapseConstants.EnvironmentVariables.Workflows.Instance.Value}'. The runner might be incorrectly configured.");
                this.ExpressionEvaluator = this.ExpressionEvaluatorFactory.Create(this.Definition);
                this.Logger.LogInformation($"Workflow context initialized");
            }   
        }

        /// <inheritdoc/>
        public virtual async Task WaitForEventsAsync(IEnumerable<EventDefinition> events, V1TriggerCorrelationMode correlationMode, V1TriggerConditionType triggerConditionType, CancellationToken cancellationToken = default)
        {
            try
            {
                using (await this.Lock.LockAsync(cancellationToken))
                {
                    this.Logger.LogInformation($"Switching to passive correlation mode...");
                    this.Instance.WaitForEvents();
                    this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                    this.Logger.LogInformation($"Creating a new V1Trigger for passive correlation...");
                    V1Trigger trigger = new(new V1TriggerSpec(correlationMode, triggerConditionType, new V1TriggerOutcome(V1TriggerOutcomeType.Resume, this.Instance.Spec.Definition, this.Instance.Name())))
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            GenerateName = "synapse-trigger-",
                            NamespaceProperty = this.Instance.Namespace()
                        }
                    };
                    trigger.SetLabel(SynapseConstants.Labels.Scheduled, true.ToString());
                    foreach (EventDefinition e in events)
                    {
                        V1TriggerCondition condition = new();
                        V1EventFilter filter = new();
                        filter.ContextAttributes.Add(CloudEventAttributes.TypeAttributeName(), e.Type);
                        filter.ContextAttributes.Add(CloudEventAttributes.SourceAttributeName(), e.Source);
                        if (e.Correlations != null)
                        {
                            foreach (EventCorrelationDefinition correlationDefinition in e.Correlations)
                            {
                                string desiredValue = correlationDefinition.ContextAttributeValue;
                                if (this.Instance.Status.CorrelationContext.ContextAttributes.TryGetValue(correlationDefinition.ContextAttributeName, out string currentValue))
                                    desiredValue = currentValue;
                                filter.Correlations.Add(correlationDefinition.ContextAttributeName, desiredValue);
                            }
                        }
                        condition.Filters.Add(filter);
                        trigger.Spec.Conditions.Add(condition);
                    }
                    trigger = await this.Triggers.AddAsync(trigger, cancellationToken);
                    trigger.Initialize();
                    trigger = await this.Triggers.UpdateAsync(trigger, cancellationToken);
                    trigger.AddCorrelationContext(this.Instance.Status.CorrelationContext);
                    trigger = await this.Triggers.UpdateAsync(trigger, cancellationToken);
                    this.Logger.LogInformation($"V1Trigger '{trigger.Name()}' successfully created");
                    this.Logger.LogInformation($"Successfully switched to passive correlation mode. The Runner will now shutdown.");
                    this.ApplicationLifetime.StopApplication();
                }
            }
            catch(HttpOperationException ex)
            {
                //TODO: remove
                this.Logger.LogError(ex.Response.Content);
            }
        }

        /// <inheritdoc/>
        public virtual async Task StartWorkflowAsync(CancellationToken cancellationToken = default)
        {
            using (await Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation("Starting workflow instance");
                this.Instance.Start();
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Workflow instance started");
            }
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
        {
            using (await Lock.LockAsync(cancellationToken))
            {
                if (this.Instance.Status.Type == V1WorkflowActivityStatus.Executed)
                    return;
                this.Instance.Execute();
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Executing workflow instance");
            }
        }

        /// <inheritdoc/>
        public virtual async Task SuspendWorkflowAsync(CancellationToken cancellationToken = default)
        {
            using (await Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation("Suspending workflow instance execution...");
                this.Instance.Suspend();
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Workflow instance execution suspended");
            }
        }

        /// <inheritdoc/>
        public virtual async Task FaultWorkflowAsync(Exception ex, CancellationToken cancellationToken = default)
        {
            using (await Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation("Faulting workflow instance execution...");
                this.Instance.Fault(ex);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Workflow instance execution faulted");
            }
        }

        /// <inheritdoc/>
        public virtual async Task TerminateWorkflowAsync(CancellationToken cancellationToken = default)
        {
            using (await Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation("Terminating workflow instance execution...");
                this.Instance.Terminate();
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Workflow instance execution terminated");
            }
        }

        /// <inheritdoc/>
        public virtual async Task TransitionToAsync(StateDefinition state, CancellationToken cancellationToken = default)
        {
            using (await Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation("Transitioning to state '{state}'...", state.Name);
                this.Instance.TransitionTo(state);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Transitioned to state '{state}'", state.Name);
            }
        }

        /// <inheritdoc/>
        public virtual async Task SetWorkflowOutputAsync(JToken output, CancellationToken cancellationToken = default)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Logger.LogInformation("Setting workflow instance output...");
                this.Instance.SetOutput(output);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                this.Logger.LogInformation("Workflow instance output set");
            }
        }

        /// <inheritdoc/>
        public virtual async Task SetCorrelationKeyAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.SetCorrelationKey(key, value);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            }  
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryCorrelateAsync(CloudEvent e, IEnumerable<string> contextAttributes, CancellationToken cancellationToken)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                if (!this.Instance.Status.CorrelationContext.CorrelatesTo(e))
                    return false;
                this.Instance.Correlate(V1CloudEvent.CreateFor(e, this.CloudEventFormatter), contextAttributes);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<CloudEvent> GetBoostrapEventAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
        {
            if (eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                V1CloudEvent e = this.Instance.Status.CorrelationContext.BootstrapEvents.FirstOrDefault(e => e.Matches(eventDefinition));
                if (e == null)
                    return null;
                this.Instance.ConsumeBootstrapEvent(e);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return e.ToCloudEvent(this.CloudEventFormatter);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> ListActiveChildActivitiesAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return await this.ListChildActivitiesAsync(activity, false, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> ListChildActivitiesAsync(CancellationToken cancellationToken = default)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                return this.Instance.Status.ActivityLog.ToList().Where(a => a.IsActive && !a.ParentId.HasValue).ToList();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> ListChildActivitiesAsync(V1WorkflowActivity activity, bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                return await Task.Run(() => this.Instance.Status.ActivityLog.ToList().Where(a => (includeInactive || a.IsActive) && a.ParentId.HasValue && a.ParentId == activity.Id).ToList(), cancellationToken);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CreateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.AddActivity(activity);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> InitializeActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.InitializeActivity(activity);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> ExecuteActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.ExecuteActivity(activity);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SuspendActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.SuspendActivity(activity);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> FaultActivityAsync(V1WorkflowActivity activity, Exception ex, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            if (activity.Status == V1WorkflowActivityStatus.Faulted)
                return activity;
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.FaultActivity(activity, ex);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> TerminateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.TerminateActivity(activity);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SetActivityResultAsync(V1WorkflowActivity activity, V1WorkflowExecutionResult result, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.SetActivityResult(activity, result);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> UpdateActivityDataAsync(V1WorkflowActivity activity, JToken data, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Instance.UpdateActivityData(activity, data);
                this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
                return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> GetParentActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (!activity.ParentId.HasValue)
                return null;
            return await Task.Run(() => this.Instance.Status.ActivityLog.FirstOrDefault(a => a.Id == activity.ParentId.Value), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<JToken> GetActivityStateDataAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            V1WorkflowActivity parentActivity = activity;
            while (parentActivity != null)
            {
                if (parentActivity.Type == V1WorkflowActivityType.State)
                    return parentActivity.Data;
                parentActivity = await this.GetParentActivityAsync(parentActivity, cancellationToken);
            }
            return null;
        }

    }

}
