// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Resources;
using Synapse.Runtime.Services;
using System.Collections.Concurrent;

namespace Synapse.Runtime.Kubernetes.Services;

/// <summary>
/// Represents the Kubernetes implementation of the <see cref="IWorkflowRuntime"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="environment">The current <see cref="IHostEnvironment"/></param>
/// <param name="runner">The service used to access the current <see cref="RunnerConfiguration"/></param>
public class KubernetesRuntime(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IHostEnvironment environment, IOptions<RunnerConfiguration> runner)
    : WorkflowRuntimeBase(loggerFactory)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the current <see cref="IHostEnvironment"/>
    /// </summary>
    protected IHostEnvironment Environment { get; } = environment;

    /// <summary>
    /// Gets the current <see cref="RunnerConfiguration"/>
    /// </summary>
    protected RunnerConfiguration Runner => runner.Value;

    /// <summary>
    /// Gets the service used to interact with the Kubernetes API
    /// </summary>
    protected IKubernetes? Kubernetes { get; set; }

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all managed runner processes
    /// </summary>
    protected ConcurrentDictionary<string, KubernetesWorkflowProcess> Processes { get; } = new();

    /// <summary>
    /// Initializes the <see cref="KubernetesRuntime"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.Runner.Runtime.Kubernetes == null) throw new NullReferenceException($"Failed to initialize the Kubernetes Runtime because the operator is not configured to use Kubernetes as a runtime");
        var kubeconfig = string.IsNullOrWhiteSpace(this.Runner.Runtime.Kubernetes.Kubeconfig) 
            ? KubernetesClientConfiguration.InClusterConfig() 
            : await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(new FileInfo(this.Runner.Runtime.Kubernetes.Kubeconfig)).ConfigureAwait(false);
        this.Kubernetes = new k8s.Kubernetes(kubeconfig);
    }

    /// <inheritdoc/>
    public override async Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, ServiceAccount serviceAccount, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(workflowInstance);
        ArgumentNullException.ThrowIfNull(serviceAccount);
        try
        {
            this.Logger.LogDebug("Creating a new Kubernetes pod for workflow instance '{workflowInstance}'...", workflowInstance.GetQualifiedName());
            if (this.Kubernetes == null) await this.InitializeAsync(cancellationToken).ConfigureAwait(false);
            var workflowDefinition = workflow.Spec.Versions.Get(workflowInstance.Spec.Definition.Version) ?? throw new NullReferenceException($"Failed to find version '{workflowInstance.Spec.Definition.Version}' of workflow '{workflow.GetQualifiedName()}'");
            var pod = this.Runner.Runtime.Kubernetes!.PodTemplate.Clone()!;
            pod.Metadata ??= new();
            pod.Metadata.Name = $"{workflowInstance.GetQualifiedName()}-{Guid.NewGuid().ToString("N")[..12].ToLowerInvariant()}";
            if (!string.IsNullOrWhiteSpace(this.Runner.Runtime.Kubernetes.Namespace)) pod.Metadata.NamespaceProperty = this.Runner.Runtime.Kubernetes.Namespace;
            if (pod.Spec == null || pod.Spec.Containers == null || !pod.Spec.Containers.Any()) throw new InvalidOperationException("The specified Kubernetes runtime pod template is not valid");
            var volumeMounts = new List<V1VolumeMount>();
            pod.Spec.Volumes ??= [];
            if (workflowDefinition.Use?.Secrets?.Count > 0)
            {
                var secretsVolume = new V1Volume(this.Runner.Runtime.Kubernetes.Secrets.VolumeName)
                {
                    Projected = new()
                    {
                        Sources = []
                    }
                };
                pod.Spec.Volumes.Add(secretsVolume);
                var secretsVolumeMount = new V1VolumeMount(this.Runner.Runtime.Kubernetes.Secrets.MountPath, secretsVolume.Name, readOnlyProperty: true);
                volumeMounts.Add(secretsVolumeMount);
                foreach (var secret in workflowDefinition.Use.Secrets)
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
                container.Env ??= [];
                container.Env.Add(new(SynapseDefaults.EnvironmentVariables.Runner.Namespace, valueFrom: new(fieldRef: new("metadata.namespace"))));
                container.Env.Add(new(SynapseDefaults.EnvironmentVariables.Runner.Name, valueFrom: new(fieldRef: new("metadata.name"))));
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Uri, this.Runner.Api.Uri.OriginalString);
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.ContainerPlatform, this.Runner.ContainerPlatform);
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.LifecycleEvents, (this.Runner.PublishLifecycleEvents ?? true).ToString());
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Secrets.Directory, this.Runner.Runtime.Kubernetes.Secrets.MountPath);
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.ServiceAccount.Name, serviceAccount.GetQualifiedName());
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.ServiceAccount.Key, serviceAccount.Spec.Key);
                container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Workflow.Instance, workflowInstance.GetQualifiedName());
                if (this.Runner.Certificates?.Validate == false) container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.SkipCertificateValidation, "true");
                container.VolumeMounts = volumeMounts;
            }
            if(this.Runner.ContainerPlatform == ContainerPlatform.Kubernetes) pod.Spec.ServiceAccountName = this.Runner.Runtime.Kubernetes.ServiceAccount;
            var process = ActivatorUtilities.CreateInstance<KubernetesWorkflowProcess>(this.ServiceProvider, this.Kubernetes!, pod);
            this.Processes.AddOrUpdate(process.Id, _ => process, (key, current) =>
            {
                current.StopAsync().GetAwaiter().GetResult();
#pragma warning disable CA2012 // Use ValueTasks correctly
                current.DisposeAsync().GetAwaiter().GetResult();
#pragma warning restore CA2012 // Use ValueTasks correctly
                return process;
            });
            this.Logger.LogDebug("A new container with id '{id}' has been successfully created to run workflow instance '{workflowInstance}'", process.Id, workflowInstance.GetQualifiedName());
            return process;
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while creating a new Kubernetes process for workflow instance '{workflowInstance}': {ex}", workflowInstance.GetQualifiedName(), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    protected override ValueTask DisposeAsync(bool disposing)
    {
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        this.Kubernetes?.Dispose();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        this.Kubernetes?.Dispose();
        base.Dispose(disposing);
    }

}
