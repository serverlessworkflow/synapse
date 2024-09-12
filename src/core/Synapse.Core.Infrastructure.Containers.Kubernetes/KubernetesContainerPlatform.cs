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
using ServerlessWorkflow.Sdk.Models.Processes;
using Synapse.Core.Infrastructure.Containers.Configuration;
using Synapse.Core.Infrastructure.Services;

namespace Synapse.Core.Infrastructure.Containers;

/// <summary>
/// Represents the Docker implementation of the <see cref="IContainerPlatform"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="hostEnvironment">The current <see cref="IHostEnvironment"/></param>
/// <param name="options">The current <see cref="KubernetesContainerPlatformOptions"/></param>
public class KubernetesContainerPlatform(IServiceProvider serviceProvider, ILogger<KubernetesContainerPlatform> logger, IHostEnvironment hostEnvironment, IOptions<KubernetesContainerPlatformOptions> options)
    : IHostedService, IContainerPlatform, IDisposable, IAsyncDisposable
{

    bool _disposed;

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the current <see cref="IHostEnvironment"/>
    /// </summary>
    protected IHostEnvironment Environment { get; } = hostEnvironment;

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected IKubernetes? Kubernetes { get; set; }

    /// <summary>
    /// Gets the current <see cref="KubernetesContainerPlatformOptions"/>
    /// </summary>
    protected KubernetesContainerPlatformOptions Options { get; } = options.Value;

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        var kubeconfig = string.IsNullOrWhiteSpace(this.Options.Kubeconfig)
            ? KubernetesClientConfiguration.InClusterConfig()
            : await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(new FileInfo(this.Options.Kubeconfig)).ConfigureAwait(false);
        this.Kubernetes = new Kubernetes(kubeconfig);
    }

    /// <inheritdoc/>
    public virtual Task<IContainer> CreateAsync(ContainerProcessDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (this.Kubernetes == null) throw new NullReferenceException("The KubernetesContainerPlatform has not been properly initialized");
        var pod = new V1Pod()
        {
            Metadata = new()
            {
                NamespaceProperty = $"{System.Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Namespace)}",
                Name = $"{System.Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Name)}-{definition.Image}-{Guid.NewGuid().ToString("N")[..6].ToLowerInvariant()}"
            },
            Spec = new()
            {
                RestartPolicy = "Never",
                Containers = 
                [
                    new(definition.Image)
                    {
                        Image = definition.Image,
                        ImagePullPolicy = this.Options.ImagePullPolicy,
                        Command = string.IsNullOrWhiteSpace(definition.Command) ? null : ["/bin/sh", "-c", definition.Command],
                        Env = definition.Environment?.Select(e => new V1EnvVar(e.Key, e.Value)).ToList()
                    }
                ]
            }
        };
        return Task.FromResult((IContainer)ActivatorUtilities.CreateInstance<KubernetesContainer>(this.ServiceProvider, pod, this.Kubernetes));
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Disposes of the <see cref="KubernetesContainerPlatform"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="KubernetesContainerPlatform"/> is being disposed of</param>
    /// <returns>A new <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (this._disposed) return;
        if (disposing) this.Kubernetes?.Dispose();
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="KubernetesContainerPlatform"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="KubernetesContainerPlatform"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;
        if (disposing) this.Kubernetes?.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
