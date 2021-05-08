using CloudNative.CloudEvents;
using FluentValidation.Results;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Neuroglia.K8s;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Operator.Application.Configuration;
using Synapse.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Operator.Application.Services
{

    /// <summary>
    /// Represents the service used to manage <see cref="V1Workflow"/>s
    /// </summary>
    public class V1WorkflowController
        : ResourceController<V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowController"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="kubernetes">The service used to interact with Kubernetes</param>
        /// <param name="resourceWatcherFactory">The service used to create <see cref="IResourceWatcher"/>s</param>
        /// <param name="options">The current <see cref="ResourceControllerOptions{TResource}"/></param>
        /// <param name="applicationOptions">The current <see cref="Configuration.ApplicationOptions"/></param>
        /// <param name="workflows">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="triggers">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1Trigger"/>s</param>
        public V1WorkflowController(ILoggerFactory loggerFactory, IKubernetes kubernetes, IResourceWatcherFactory resourceWatcherFactory, IOptions<ResourceControllerOptions<V1Workflow>> options, IOptions<ApplicationOptions> applicationOptions, 
            IRepository<V1Workflow> workflows, IRepository<V1WorkflowInstance> workflowInstances, IRepository<V1Trigger> triggers)
           : base(loggerFactory, kubernetes, resourceWatcherFactory, options)
        {
            this.ApplicationOptions = applicationOptions.Value;
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
            this.Triggers = triggers;
        }

        /// <summary>
        /// Gets the current <see cref="Configuration.ApplicationOptions"/>
        /// </summary>
        protected ApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1Trigger"/>s
        /// </summary>
        protected IRepository<V1Trigger> Triggers { get; }

        /// <inheritdoc/>
        protected override void OnEvent(IResourceEvent<V1Workflow> e)
        {
            base.OnEvent(e);
            switch (e.Type)
            {
                case WatchEventType.Added:
                    _ = this.ProcessAsync(e.Resource);
                    break;
                case WatchEventType.Deleted:
                    _ = this.CleanUpAsync(e.Resource);
                    break;
            }
        }

        /// <inheritdoc/>
        public override async Task ReconcileAsync(CancellationToken cancellationToken = default)
        {
            KubernetesList<V1Workflow> workflowResources = await this.Kubernetes.ListNamespacedCustomObjectAsync<V1Workflow>(this.ResourceDefinition.Group, this.ResourceDefinition.Version, SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value, this.ResourceDefinition.Plural, cancellationToken: cancellationToken);
            foreach (V1Workflow workflowResource in workflowResources.Items
                .Where(w => w.Status == null || w.Status.Type == V1WorkflowDefinitionStatus.Pending))
            {
                await this.ProcessAsync(workflowResource, cancellationToken);
            }
        }

        /// <summary>
        /// Processes the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ProcessAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (workflow.Status != null && workflow.Status.Type == V1WorkflowDefinitionStatus.Pending)
                return;
            try
            {
                this.Logger.LogInformation("Processing workflow '{workflowId}:{workflowVersion}'...", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
                workflow = await this.InitializeAsync(workflow, cancellationToken);
                workflow = await this.ValidateAsync(workflow, cancellationToken);
                workflow = await this.DeployAsync(workflow, cancellationToken);
                this.Logger.LogInformation("Workflow '{workflowId}:{workflowVersion}' successfully processed", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
            }
            catch (Exception ex)
            {
                if (ex is HttpOperationException httpEx)
                    this.Logger.LogError($"An error occured while processing the Workflow Custom Resource with name '{{resourceName}}: the Kubernetes API returned an non-success status code '{{statusCode}}'{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", workflow.Name(), httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"An error occured while processing the Workflow Custom Resource with name '{{resourceName}}':{Environment.NewLine}{{ex}}", workflow.Name(), ex.ToString());
                workflow.Fault(ex);
                await this.Workflows.UpdateAsync(workflow, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Initializes the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to initialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1Workflow"/></returns>
        protected virtual async Task<V1Workflow> InitializeAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            this.Logger.LogInformation("Initialzing workflow '{workflowId}:{workflowVersion}'...", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
            workflow.StartProcessing();
            workflow = await this.Workflows.UpdateAsync(workflow, cancellationToken);
            workflow.Label();
            workflow = await this.Workflows.UpdateAsync(workflow, cancellationToken);
            this.Logger.LogInformation("Workflow '{workflowId}:{workflowVersion}' successfully initialized", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
            return workflow;
        }

        /// <summary>
        /// Validates the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to validate</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1Workflow"/></returns>
        protected virtual async Task<V1Workflow> ValidateAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            this.Logger.LogInformation("Validating workflow '{workflowId}:{workflowVersion}'...", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
            //TODO: validation
            ValidationResult validationResult = new();
            workflow.SetValidationResult(validationResult);
            workflow = await this.Workflows.UpdateAsync(workflow, cancellationToken);
            this.Logger.LogInformation("Workflow '{workflowId}:{workflowVersion}' validation completed: [{validationResult}]", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version, validationResult.IsValid ? "VALID" : "INVALID");
            return workflow;
        }

        /// <summary>
        /// Deploys the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to deploy</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1Workflow"/></returns>
        protected virtual async Task<V1Workflow> DeployAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            StateDefinition startupState = workflow.Spec.Definition.GetStartupState();
            if (startupState is EventStateDefinition 
                || (startupState is SwitchStateDefinition switchState && switchState.SwitchType == SwitchStateType.Event))
                workflow = await this.DeployTriggerAsync(workflow, cancellationToken);
            else if(workflow.Spec.Definition.Start.Schedule != null)
            {
                if(!string.IsNullOrWhiteSpace(workflow.Spec.Definition.Start.Schedule.Cron.Expression))
                    workflow = await this.DeployCronJobAsync(workflow, cancellationToken);
                if (!string.IsNullOrWhiteSpace(workflow.Spec.Definition.Start.Schedule.Interval))
                    workflow = await this.DeployJobAsync(workflow, cancellationToken);
            }
            return workflow;
        }

        /// <summary>
        /// Deploys a <see cref="V1Trigger"/> for the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to deploy the <see cref="V1Trigger"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1Workflow"/></returns>
        protected virtual async Task<V1Workflow> DeployTriggerAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            try
            {
                EventStateDefinition eventState = workflow.Spec.Definition.GetStartupState<EventStateDefinition>();
                this.Logger.LogInformation("The startup state '{startupState}' of workflow '{workflowId}:{workflowDefinition}' is an event state definition", eventState.Name, workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
                this.Logger.LogInformation("Creating a new V1Trigger...");
                V1TriggerSpec triggerSpec = new(V1TriggerCorrelationMode.Parallel, eventState.Exclusive ? V1TriggerConditionType.AnyOf : V1TriggerConditionType.AllOf, new V1TriggerOutcome(V1TriggerOutcomeType.Run, new V1WorkflowReference(workflow.Spec.Definition.Id, workflow.Spec.Definition.Version)));
                foreach (EventStateTriggerDefinition triggerDefinition in eventState.Triggers)
                {
                    V1TriggerCondition condition = new();
                    foreach (string eventReference in triggerDefinition.Events)
                    {
                        if (!workflow.Spec.Definition.TryGetEvent(eventReference, out EventDefinition eventDefinition))
                            throw new NullReferenceException($"Failed to find the specified event '{eventReference}' in workflow '{workflow.Spec.Definition.Id}:{workflow.Spec.Definition.Version}'");
                        V1EventFilter filter = new();
                        filter.ContextAttributes.Add(CloudEventAttributes.TypeAttributeName(), eventDefinition.Type);
                        filter.ContextAttributes.Add(CloudEventAttributes.SourceAttributeName(), eventDefinition.Source);
                        if (eventDefinition.Correlations != null)
                        {
                            foreach (EventCorrelationDefinition correlationDefinition in eventDefinition.Correlations)
                            {
                                filter.Correlations.Add(correlationDefinition.ContextAttributeName, correlationDefinition.ContextAttributeValue);
                            }
                        }
                        condition.Filters.Add(filter);
                    }
                    triggerSpec.Conditions.Add(condition);
                }
                V1Trigger trigger = new(triggerSpec)
                {
                    Metadata = new V1ObjectMeta()
                    {
                        GenerateName = "synapse-trigger-",
                        NamespaceProperty = workflow.Namespace()
                    }
                };
                trigger = await this.Triggers.AddAsync(trigger, cancellationToken);
                this.Logger.LogInformation("A new V1Trigger has been successfully created for workflow '{workflowId}:{workflowDefinition}'. You can now start to send events to a Synapse Broker.", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
                return workflow;
            }
            catch (Exception ex)
            { 
                if (ex is HttpOperationException httpEx)
                    this.Logger.LogError($"An error occured while creating a V1Trigger for workflow '{{workflowId}}:{{workflowDefinition}}: the Kubernetes API returned an non-success status code '{{statusCode}}'{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version, httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"An error occured while creating a V1Trigger for Workflow '{{workflowId}}:{{workflowDefinition}}':{Environment.NewLine}{{ex}}", workflow.Name(), ex.ToString());
                workflow.Fault(ex);
                await this.Workflows.UpdateAsync(workflow, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Deploys a new <see cref="V1Job"/> for the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to deploy a new <see cref="V1Job"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1Workflow"/></returns>
        protected virtual Task<V1Workflow> DeployJobAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deploys a new <see cref="V1CronJob"/> for the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to deploy a new <see cref="V1CronJob"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="V1Workflow"/></returns>
        protected virtual async Task<V1Workflow> DeployCronJobAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            try
            {
                this.Logger.LogInformation("The start definition of workflow '{workflowId}:{workflowDefinition}' defines a CRON schedule.", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
                this.Logger.LogInformation("Creating a new CronJob...");
                V1CronJob cronJob = new()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        GenerateName = "synapse-cronjob-",
                        NamespaceProperty = workflow.Namespace()
                    },
                    Spec = new V1CronJobSpec()
                    {
                        Schedule = workflow.Spec.Definition.Start.Schedule.Cron.Expression,

                        JobTemplate = new V1JobTemplateSpec()
                        {
                            Spec = new V1JobSpec()
                            {
                                Template = new V1PodTemplateSpec()
                                {
                                    Metadata = new V1ObjectMeta()
                                    {

                                    },
                                    Spec = new V1PodSpec()
                                    {

                                    }
                                }
                            }
                        }
                    }
                };
                //TODO
                this.Logger.LogInformation("A new CronJob has been successfully created for workflow '{workflowId}:{workflowDefinition}'.", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
                return workflow;
            }
            catch (Exception ex)
            {
                if (ex is HttpOperationException httpEx)
                    this.Logger.LogError($"An error occured while scheduling workflow '{{workflowId}}:{{workflowDefinition}}: the Kubernetes API returned an non-success status code '{{statusCode}}'{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version, httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"An error occured while scheduling Workflow '{{workflowId}}:{{workflowDefinition}}':{Environment.NewLine}{{ex}}", workflow.Name(), ex.ToString());
                workflow.Fault(ex);
                await this.Workflows.UpdateAsync(workflow, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Cleans up artifacts related to the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to clean up the artifacts of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CleanUpAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            string labelSelector = $"{SynapseConstants.Labels.Workflows.Id}={workflow.Spec.Definition.Id},{SynapseConstants.Labels.Workflows.Version}={workflow.Spec.Definition.Version}";
            foreach(V1WorkflowInstance workflowInstance in await WorkflowInstances.FilterAsync(labelSelector, cancellationToken: cancellationToken))
            {
                await this.WorkflowInstances.RemoveAsync(workflowInstance, cancellationToken);
            }
        }

    }

}
