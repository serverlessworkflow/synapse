using FluentValidation.Results;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Neuroglia.K8s;
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
        public V1WorkflowController(ILoggerFactory loggerFactory, IKubernetes kubernetes, IResourceWatcherFactory resourceWatcherFactory, IOptions<ResourceControllerOptions<V1Workflow>> options, IOptions<ApplicationOptions> applicationOptions, IRepository<V1Workflow> workflows, IRepository<V1WorkflowInstance> workflowInstances)
           : base(loggerFactory, kubernetes, resourceWatcherFactory, options)
        {
            this.ApplicationOptions = applicationOptions.Value;
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
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
                this.Logger.LogInformation("Workflow '{workflowId}:{workflowVersion}' has been successfully processed", workflow.Spec.Definition.Id, workflow.Spec.Definition.Version);
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
        /// <returns>A new awaitable <see cref="Task"/></returns>
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
        /// <returns>A new awaitable <see cref="Task"/></returns>
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
        /// Cleans up artifacts related to the specified <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="V1Workflow"/> to clean up the artifacts of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CleanUpAsync(V1Workflow workflow, CancellationToken cancellationToken = default)
        {
            string labelSelector = $"{SynapseConstants.Labels.Workflows.Id}={workflow.Spec.Definition.Id},{SynapseConstants.Labels.Workflows.Version}={workflow.Spec.Definition.Version}";
            foreach(V1WorkflowInstance workflowInstance in await this.WorkflowInstances.FilterAsync(labelSelector))
            {
                await this.WorkflowInstances.RemoveAsync(workflowInstance, cancellationToken);
            }
        }

    }

}
