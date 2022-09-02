/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuroglia;
using Neuroglia.K8s;
using Newtonsoft.Json;
using Synapse.Application.Configuration;
using Synapse.Application.Services;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Services;
using Synapse.Runtime.Kubernetes.Configuration;

namespace Synapse.Runtime.Kubernetes.Services
{

    /// <summary>
    /// Represents the Kubernetes implementation of the <see cref="IWorkflowRuntime"/>
    /// </summary>
    public class KubernetesRuntimeHost
        : WorkflowRuntimeHostBase
    {

        /// <summary>
        /// Initializes a new <see cref="KubernetesRuntimeHost"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <param name="applicationOptions">The service used to access the current <see cref="SynapseApplicationOptions"/></param>
        /// <param name="options">The service used to access the current <see cref="KubernetesRuntimeOptions"/></param>
        /// <param name="kubernetes">The service used to interact with the Kubernetes API</param>
        public KubernetesRuntimeHost(ILoggerFactory loggerFactory, IHostEnvironment environment, 
            IOptions<SynapseApplicationOptions> applicationOptions, IOptions<KubernetesRuntimeOptions> options, IKubernetes kubernetes)
            : base(loggerFactory)
        {
            this.Environment = environment;
            this.ApplicationOptions = applicationOptions.Value;
            this.Options = options.Value;
            this.Kubernetes = kubernetes;
        }

        /// <summary>
        /// Gets the current <see cref="IHostEnvironment"/>
        /// </summary>
        protected IHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Gets the current <see cref="KubernetesRuntimeOptions"/>
        /// </summary>
        protected KubernetesRuntimeOptions Options { get; }

        /// <summary>
        /// Gets the service used to interact with the Kubernetes API
        /// </summary>
        protected IKubernetes Kubernetes { get; }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!this.Environment.RunsInKubernetes())
                return;
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task<IWorkflowProcess> CreateProcessAsync(V1Workflow workflow, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            var pod = this.BuildWorkerPodFor(workflow, workflowInstance);
            return Task.FromResult<IWorkflowProcess>(new KubernetesProcess(pod, this.Kubernetes));
        }

        /// <summary>
        /// Builds a new worker <see cref="V1Pod"/> for the specified <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="workflow">The instanciated <see cref="V1Workflow"/> to build a new worker <see cref="V1Pod"/> for</param>
        /// <param name="workflowInstance">The <see cref="V1WorkflowInstance"/> to build a new worker <see cref="V1Pod"/> for</param>
        /// <returns>A new worker <see cref="V1Pod"/></returns>
        protected virtual V1Pod BuildWorkerPodFor(V1Workflow workflow, V1WorkflowInstance workflowInstance)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            if (this.Options.WorkerPod == null)
                throw new Exception($"The '{nameof(KubernetesRuntimeOptions)}.{nameof(KubernetesRuntimeOptions.WorkerPod)}' property must be set and cannot be null");
            var pod = JsonConvert.DeserializeObject<V1Pod>(JsonConvert.SerializeObject(this.Options.WorkerPod))!;
            if (pod.Metadata == null)
                pod.Metadata = new();
            pod.Metadata.Name = workflowInstance.Id;
            pod.Metadata.NamespaceProperty = EnvironmentVariables.Kubernetes.Namespace.Value;
            pod.Spec.RestartPolicy = "Never";
            if (pod.Spec == null
                || pod.Spec.Containers == null
                || !pod.Spec.Containers.Any())
                throw new InvalidOperationException("The specified V1Pod is not valid");
            var volumeMounts = new List<V1VolumeMount>();
            if(pod.Spec.Volumes == null)
                pod.Spec.Volumes = new List<V1Volume>();
            if (workflow.Definition.Secrets != null
                && workflow.Definition.Secrets.Any())
            {
                var secretsVolume = new V1Volume("secrets")
                {
                    Projected = new()
                    {
                        Sources = new List<V1VolumeProjection>()
                    }
                };
                pod.Spec.Volumes.Add(secretsVolume);
                var secretsVolumeMount = new V1VolumeMount("/run/secrets/synapse", secretsVolume.Name, readOnlyProperty: true);
                volumeMounts.Add(secretsVolumeMount);
                foreach (var secret in workflow.Definition.Secrets)
                {
                    secretsVolume.Projected.Sources.Add(new() 
                    { 
                        Secret = new()
                        {
                            Name = secret,
                            Optional = false
                        }
                    });
                }
            }
            foreach (var container in pod.Spec.Containers)
            {
                if (container.Env == null)
                    container.Env = new List<V1EnvVar>();
                container.AddOrUpdateEnvironmentVariable(new(EnvironmentVariables.Api.HostName.Name, EnvironmentVariables.Api.HostName.Value!));
                container.AddOrUpdateEnvironmentVariable(new(EnvironmentVariables.Runtime.WorkflowInstanceId.Name, workflowInstance.Id));
                container.AddOrUpdateEnvironmentVariable(new(EnvironmentVariables.Kubernetes.Namespace.Name, valueFrom: new V1EnvVarSource() { FieldRef = new V1ObjectFieldSelector("metadata.namespace") }));
                container.AddOrUpdateEnvironmentVariable(new(EnvironmentVariables.Kubernetes.PodName.Name, valueFrom: new V1EnvVarSource() { FieldRef = new V1ObjectFieldSelector("metadata.name") }));
                if (this.ApplicationOptions.SkipCertificateValidation)
                    container.AddOrUpdateEnvironmentVariable(new(EnvironmentVariables.SkipCertificateValidation.Name, "true"));
                container.VolumeMounts = volumeMounts;
            }
            return pod;
        }

        /// <summary>
        /// Exposes constants used by the <see cref="KubernetesRuntimeHost"/>
        /// </summary>
        public static class Constants
        {

            /// <summary>
            /// Exposes constants about Synapse CRDs (Custom Resource Definitions)
            /// </summary>
            public static class CustomResourceDefinitions
            {

                /// <summary>
                /// Gets Synapse's resource namespace
                /// </summary>
                public const string Namespace = "synapse.io";

                /// <summary>
                /// Gets the default group for all Synapse resources
                /// </summary>
                public const string Group = Namespace;

                /// <summary>
                /// Gets the default version for all Synapse resources
                /// </summary>
                public const string Version = "v1alpha1";

                /// <summary>
                /// Gets the default api version for all Synapse resources
                /// </summary>
                public static string ApiVersion = string.Join("/", Group, Version);

                /// <summary>
                /// Gets the <see cref="Domain.Models.V1WorkflowInstance"/>'s <see cref="ICustomResourceDefinition"/>
                /// </summary>
                public static CustomResourceDefinition V1WorkflowInstance = new(ApiVersion, "WorkflowInstance", "workflow-instances");

            }

        }

    }

}
