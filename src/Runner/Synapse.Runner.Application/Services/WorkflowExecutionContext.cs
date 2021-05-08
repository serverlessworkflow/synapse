using CloudNative.CloudEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
        /// <param name="cloudEventFormatter">The service used to format <see cref="CloudEvent"/>s</param>
        public WorkflowExecutionContext(ILogger<WorkflowExecutionContext> logger, IHostApplicationLifetime applicationLifetime, IExpressionEvaluatorFactory expressionEvaluatorFactory, IRepository<V1Workflow> definitions, IRepository<V1WorkflowInstance> instances, ICloudEventFormatter cloudEventFormatter)
        {
            this.Logger = logger;
            this.ApplicationLifetime = applicationLifetime;
            this.ExpressionEvaluatorFactory = expressionEvaluatorFactory;
            this.Definitions = definitions;
            this.Instances = instances;
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

        /// <inheritdoc/>
        public V1Workflow Definition { get; private set; }

        /// <inheritdoc/>
        public V1WorkflowInstance Instance { get; private set; }

        /// <inheritdoc/>
        public IExpressionEvaluator ExpressionEvaluator { get; private set; }

        /// <inheritdoc/>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation($"Initializing the current workflow context...");
            this.Definition = await this.Definitions.FindAsync(SynapseConstants.EnvironmentVariables.Workflows.Id.Value, SynapseConstants.EnvironmentVariables.Workflows.Version.Value, cancellationToken: cancellationToken);
            if (this.Definition == null)
                throw new NullReferenceException($"Failed to find the Workflow Custom Resource '{SynapseConstants.EnvironmentVariables.Workflows.Id.Value}:{SynapseConstants.EnvironmentVariables.Workflows.Version.Value}'. The runner might be incorrectly configured.");
            this.Instance = await Instances.FindByNameAsync(SynapseConstants.EnvironmentVariables.Workflows.Instance.Value, cancellationToken);
            if (this.Instance == null)
                throw new NullReferenceException($"Failed to find the Workflow Instance Custom Resource '{SynapseConstants.EnvironmentVariables.Workflows.Instance.Value}'. The runner might be incorrectly configured.");
            this.ExpressionEvaluator = this.ExpressionEvaluatorFactory.Create(this.Definition);
            this.Logger.LogInformation($"Workflow context initialized");
        }

        /// <inheritdoc/>
        public virtual async Task StartWorkflowAsync(CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Starting workflow instance");
            this.Instance.Start();
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Workflow instance started");
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default)
        {
            this.Instance.Execute();
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Executing workflow instance");
        }

        /// <inheritdoc/>
        public virtual async Task SuspendWorkflowAsync(CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Suspending workflow instance execution...");
            this.Instance.Suspend();
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Workflow instance execution suspended");
        }

        /// <inheritdoc/>
        public virtual async Task FaultWorkflowAsync(Exception ex, CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Faulting workflow instance execution...");
            this.Instance.Fault(ex);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Workflow instance execution faulted");
        }

        /// <inheritdoc/>
        public virtual async Task TerminateWorkflowAsync(CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Terminating workflow instance execution...");
            this.Instance.Terminate();
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Workflow instance execution terminated");
        }

        /// <inheritdoc/>
        public virtual async Task TransitionToAsync(StateDefinition state, CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Transitioning to state '{state}'...", state.Name);
            this.Instance.TransitionTo(state);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Transitioned to state '{state}'", state.Name);
        }

        /// <inheritdoc/>
        public virtual async Task SetWorkflowOutputAsync(JToken output, CancellationToken cancellationToken = default)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            this.Logger.LogInformation("Setting workflow instance output...");
            this.Instance.SetOutput(output);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            this.Logger.LogInformation("Workflow instance output set");
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryCorrelateAsync(CloudEvent e, IEnumerable<string> contextAttributes, CancellationToken cancellationToken)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (!this.Instance.Status.CorrelationContext.CorrelatesTo(e))
                return false;
            this.Instance.Correlate(V1CloudEvent.CreateFor(e, this.CloudEventFormatter), contextAttributes);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<CloudEvent> GetBoostrapEventAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
        {
            if (eventDefinition == null)
                throw new ArgumentNullException(nameof(eventDefinition));
            V1CloudEvent e = this.Instance.Status.CorrelationContext.BootstrapEvents.FirstOrDefault(e => e.Matches(eventDefinition));
            if (e == null)
                return null;
            this.Instance.ConsumeBootstrapEvent(e);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return e.ToCloudEvent(this.CloudEventFormatter);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> ListChildActivitiesAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => this.Instance.Status.ActivityLog.ToList().Where(a => a.Status == V1WorkflowActivityStatus.Pending && !a.ParentId.HasValue).ToList(), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> ListActiveChildActivitiesAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return await this.ListChildActivitiesAsync(activity, false, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> ListChildActivitiesAsync(V1WorkflowActivity activity, bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return await Task.Run(() => this.Instance.Status.ActivityLog.ToList().Where(a => includeInactive ? true : a.IsActive && a.ParentId.HasValue && a.ParentId == activity.Id).ToList(), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CreateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            this.Instance.AddActivity(activity);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> InitializeActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            this.Instance.InitializeActivity(activity);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> ProcessActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            this.Instance.ProcessActivity(activity);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SuspendActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            this.Instance.SuspendActivity(activity);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> FaultActivityAsync(V1WorkflowActivity activity, Exception ex, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            this.Instance.FaultActivity(activity, ex);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> TerminateActivityAsync(V1WorkflowActivity activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            this.Instance.TerminateActivity(activity);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SetActivityResultAsync(V1WorkflowActivity activity, V1WorkflowExecutionResult result, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            this.Instance.SetActivityResult(activity, result);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> UpdateActivityDataAsync(V1WorkflowActivity activity, JToken data, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            this.Instance.UpdateActivityData(activity, data);
            this.Instance = await this.Instances.UpdateAsync(this.Instance, cancellationToken);
            return this.Instance.Status.ActivityLog.Single(a => a.Id == activity.Id);
        }
    }

}
