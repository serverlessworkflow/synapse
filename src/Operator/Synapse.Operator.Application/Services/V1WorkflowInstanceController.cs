using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Neuroglia.K8s;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Operator.Application.Configuration;
using Synapse.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Operator.Application.Services
{

    public class V1WorkflowInstanceController
        : ResourceController<V1WorkflowInstance>
    {

        public V1WorkflowInstanceController(ILoggerFactory loggerFactory, IKubernetes kubernetes, IResourceWatcherFactory resourceWatcherFactory, IOptions<ResourceControllerOptions<V1WorkflowInstance>> options, 
            IOptions<ApplicationOptions> applicationOptions, IRepository<V1Workflow> workflows, IRepository<V1WorkflowInstance> workflowInstances) 
            : base(loggerFactory, kubernetes, resourceWatcherFactory, options)
        {
            this.ApplicationOptions = applicationOptions.Value;
            this.Workflows = workflows;
            this.WorkflowInstances = workflowInstances;
        }

        protected ApplicationOptions ApplicationOptions { get; }

        protected IRepository<V1Workflow> Workflows { get; }

        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        protected override void OnEvent(IResourceEvent<V1WorkflowInstance> e)
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

        public override async Task ReconcileAsync(CancellationToken cancellationToken = default)
        {
            KubernetesList<V1WorkflowInstance> workflowInstanceResources = await this.Kubernetes.ListClusterCustomObjectAsync<V1WorkflowInstance>(this.ResourceDefinition.Group, this.ResourceDefinition.Version, this.ResourceDefinition.Plural, cancellationToken: cancellationToken);
            foreach (V1WorkflowInstance workflowInstanceResource in (await this.WorkflowInstances.ToListAsync(cancellationToken))
                .Where(w => w.Status == null || w.Status.Type == V1WorkflowActivityStatus.Pending))
            {
                await this.ProcessAsync(workflowInstanceResource, cancellationToken);
            }
        }

        protected virtual async Task ProcessAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            try
            {
                if (workflowInstance == null)
                    throw new ArgumentNullException(nameof(workflowInstance));
                V1Workflow workflowDefinition = await this.Workflows.FindAsync(workflowInstance.Spec.Definition.Id, workflowInstance.Spec.Definition.Version, cancellationToken);
                if(workflowDefinition == null)
                {
                    this.Logger.LogError("Failed to find the specified Workflow Custom Resource '{workflowDefinitionId}:{workflowDefinitionVersion}'", workflowInstance.Spec.Definition.Id, workflowInstance.Spec.Definition.Version);
                    throw new NullReferenceException($"Failed to find the specified Workflow Custom Resource '{workflowInstance.Spec.Definition.Id}:{workflowInstance.Spec.Definition.Version}'");
                }

                workflowInstance = await this.InitializeAsync(workflowInstance, workflowDefinition, cancellationToken);

                if (workflowDefinition.Spec.Definition.GetStartupState() is EventStateDefinition eventState)
                    workflowInstance = await this.CreateEventTriggerAsync(workflowInstance, cancellationToken);
                else
                    workflowInstance = await this.DeployAsync(workflowInstance, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is HttpOperationException httpEx)
                    this.Logger.LogError($"An error occured while processing the Workflow Instance Custom Resource with name '{{resourceName}}: the Kubernetes API returned an non-success status code '{{statusCode}}'{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", workflowInstance.Name(), httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"An error occured while processing the Workflow Instance Custom Resource with name '{{resourceName}}':{Environment.NewLine}{{ex}}", workflowInstance.Name(), ex.ToString());
                workflowInstance.Fault(ex);
                await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
                throw;
            }
        }

        protected virtual async Task<V1WorkflowInstance> InitializeAsync(V1WorkflowInstance workflowInstance, V1Workflow workflowDefinition, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (workflowDefinition == null)
                throw new ArgumentNullException(nameof(workflowDefinition));
            workflowInstance.Initialize();
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            workflowInstance.SetVersion(workflowDefinition.Spec.Definition.Version);
            workflowInstance.Label();
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        protected virtual async Task<V1WorkflowInstance> CreateEventTriggerAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            //TODO
            //workflowInstance.WaitForEvents();
            //workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        protected virtual async Task<V1WorkflowInstance> DeployAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            V1Pod pod = await this.BuildRunnerPodAsync(workflowInstance, cancellationToken);
            pod = await this.Kubernetes.CreateNamespacedPodAsync(pod, SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value, cancellationToken: cancellationToken);
            workflowInstance.Deploy(pod.Name());
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            return workflowInstance;
        }

        protected virtual async Task<V1Pod> BuildRunnerPodAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            string deploymentFilePath = this.ApplicationOptions.Runner.DeploymentFilePath;
            if (!File.Exists(deploymentFilePath))
            {
                deploymentFilePath = RunnerOptions.DefaultDeploymentFilePath;
                this.Logger.LogWarning($"Failed to find the specified pod declaration file '{{filePath}}'. Using default file path instead ('{RunnerOptions.DefaultDeploymentFilePath}').", deploymentFilePath);
            }
            string json;
            using (FileStream stream = new(deploymentFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new(stream))
                {
                    json = await streamReader.ReadToEndAsync();
                }
            }
            V1Pod pod = Yaml.LoadFromString<V1Pod>(json);
            pod.Spec.RestartPolicy = "Never";
            if (pod.Spec == null
                || pod.Spec.Containers == null
                || !pod.Spec.Containers.Any())
                throw new InvalidOperationException($"The specified pod declaration file '{deploymentFilePath}' does not contain a valid K8s Pod declaration");
            foreach (V1Container container in pod.Spec.Containers)
            {
                if (container.Env == null)
                    container.Env = new List<V1EnvVar>();
                container.Env.Add(new V1EnvVar(SynapseConstants.EnvironmentVariables.Workflows.Id.Name, workflowInstance.Spec.Definition.Id));
                container.Env.Add(new V1EnvVar(SynapseConstants.EnvironmentVariables.Workflows.Version.Name, workflowInstance.GetLabel(SynapseConstants.Labels.Workflows.Version)));
                container.Env.Add(new V1EnvVar(SynapseConstants.EnvironmentVariables.Workflows.Instance.Name, workflowInstance.Name()));
                container.Env.Add(new V1EnvVar(SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Name, valueFrom: new V1EnvVarSource() { FieldRef = new V1ObjectFieldSelector("metadata.namespace") }));
                container.Env.Add(new V1EnvVar(SynapseConstants.EnvironmentVariables.Kubernetes.PodName.Name, valueFrom: new V1EnvVarSource() { FieldRef = new V1ObjectFieldSelector("metadata.name") }));
            }
            return pod;
        }

        /// <summary>
        /// Cleans up artifacts related to the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to clean up the artifacts of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CleanUpAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (workflowInstance.Status == null
                || workflowInstance.Status.Pod == null)
                return;
            await this.Kubernetes.DeleteNamespacedPodAsync(workflowInstance.Status.Pod.Name, workflowInstance.Namespace());
        }

    }

}
