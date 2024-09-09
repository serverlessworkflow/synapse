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
using Microsoft.Extensions.Logging;
using Synapse.Core.Infrastructure.Services;

namespace Synapse.Core.Infrastructure.Containers;

/// <summary>
/// Represents a Kubernetes <see cref="IContainer"/>
/// </summary>
/// <param name="pod">The <see cref="V1Pod"/> the <see cref="IContainer"/> belongs to</param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="kubernetes">The service used to interact with the Docker API</param>
public class KubernetesContainer(V1Pod pod, ILogger<KubernetesContainer> logger, IKubernetes kubernetes)
    : IContainer
{

    bool _disposed;

    /// <summary>
    /// Gets the <see cref="V1Pod"/> the container belongs to
    /// </summary>
    protected V1Pod Pod { get; set; } = pod;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected virtual IKubernetes Kubernetes { get; } = kubernetes;

    /// <inheritdoc/>
    public virtual StreamReader? StandardOutput { get; protected set; }

    /// <inheritdoc/>
    public virtual StreamReader? StandardError { get; protected set; }

    /// <inheritdoc/>
    public long? ExitCode { get; protected set; }

    /// <summary>
    /// Gets the <see cref="KubernetesContainer"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this.Logger.LogDebug("Creating pod '{pod}'...", $"{this.Pod.Name()}.{this.Pod.Namespace()}");
            this.Pod = await this.Kubernetes.CoreV1.CreateNamespacedPodAsync(this.Pod, this.Pod.Namespace(), cancellationToken: cancellationToken);
            this.Logger.LogDebug("The pod '{pod}' has been successfully created", $"{this.Pod.Name()}.{this.Pod.Namespace()}");
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while creating the specified pod '{pod}': {ex}", $"{this.Pod.Name()}.{this.Pod.Namespace()}", ex);
        }
        await this.ReadPodLogsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the logs of the container's pod
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ReadPodLogsAsync(CancellationToken cancellationToken = default)
    {
        await this.WaitForReadyAsync(cancellationToken);
        var logStream = await this.Kubernetes.CoreV1.ReadNamespacedPodLogAsync(this.Pod.Name(), this.Pod.Namespace(), cancellationToken: cancellationToken).ConfigureAwait(false);
       this.StandardOutput = new StreamReader(logStream);
    }

    /// <summary>
    /// Waits until the pod becomes available
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task WaitForReadyAsync(CancellationToken cancellationToken = default)
    {
        this.Logger.LogDebug("Waiting for pod '{pod}'...", $"{this.Pod.Name()}.{this.Pod.Namespace()}");
        this.Pod = await this.Kubernetes.CoreV1.ReadNamespacedPodAsync(this.Pod.Name(), this.Pod.Namespace(), cancellationToken: cancellationToken);
        while (this.Pod.Status.Phase == "Pending")
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            this.Pod = await this.Kubernetes.CoreV1.ReadNamespacedPodAsync(this.Pod.Name(), this.Pod.Namespace(), cancellationToken: cancellationToken);
        }
        this.Logger.LogDebug("The pod '{pod}' is up and running", $"{this.Pod.Name()}.{this.Pod.Namespace()}");
    }

    /// <inheritdoc/>
    public virtual async Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        var response = this.Kubernetes.CoreV1.ListNamespacedPodWithHttpMessagesAsync(this.Pod.Namespace(), fieldSelector: $"metadata.name={Pod.Name()}", cancellationToken: cancellationToken);
        await foreach (var (_, item) in response.WatchAsync<V1Pod, V1PodList>(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (item.Status.Phase != "Succeeded" || item.Status.Phase != "Failed") continue;
            var containerStatus = item.Status.ContainerStatuses.FirstOrDefault();
            this.ExitCode = containerStatus?.State.Terminated?.ExitCode ?? -1;
            break;
        }
    }

    /// <inheritdoc/>
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.Kubernetes.CoreV1.DeleteNamespacedPodAsync(this.Pod.Name(), this.Pod.Namespace(), cancellationToken: cancellationToken).ConfigureAwait(false);
        await this.CancellationTokenSource.CancelAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="KubernetesContainer"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="KubernetesContainer"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed) return;
        this.StandardOutput?.Dispose();
        this.StandardError?.Dispose();
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="KubernetesContainer"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="KubernetesContainer"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed) return;
        this.StandardOutput?.Dispose();
        this.StandardError?.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}