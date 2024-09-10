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
using Synapse.Runtime.Services;
using System.Reactive.Subjects;

namespace Synapse.Runtime.Kubernetes.Services;

/// <summary>
/// Represents the Kubernetes implementation of the <see cref="IWorkflowProcess"/> interface
/// </summary>
/// <param name="pod">The <see cref="V1Pod"/> associated with the <see cref="IWorkflowProcess"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="kubernetes">The service used to interact with the Kubernetes API</param>
public class KubernetesWorkflowProcess(V1Pod pod, ILogger<KubernetesWorkflowProcess> logger, IKubernetes kubernetes)
    : WorkflowProcessBase
{

    long? _exitCode;

    /// <inheritdoc/>
    public override string Id => $"{this.Pod.Name()}.{this.Pod.Namespace()}";

    /// <summary>
    /// Gets the <see cref="V1Pod"/> the container belongs to
    /// </summary>
    protected V1Pod Pod { get; set; } = pod;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to interact with the Kubernetes API
    /// </summary>
    protected virtual IKubernetes Kubernetes { get; } = kubernetes;

    /// <inheritdoc/>
    public override IObservable<string>? StandardOutput => this.StandardOutputSubject;

    /// <summary>
    /// Gets the <see cref="Subject{T}"/> used to observe the process's STDOUT
    /// </summary>
    protected virtual Subject<string> StandardOutputSubject { get; } = new();

    /// <inheritdoc/>
    public override IObservable<string>? StandardError => this.StandardErrorSubject;

    /// <summary>
    /// Gets the <see cref="Subject{T}"/> used to observe the process's STDERR
    /// </summary>
    protected virtual Subject<string> StandardErrorSubject { get; } = new();

    /// <inheritdoc/>
    public override long? ExitCode => this._exitCode;

    /// <summary>
    /// Gets the <see cref="KubernetesWorkflowProcess"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this.Logger.LogDebug("Creating pod '{pod}'...", $"{this.Pod.Name()}.{this.Pod.Namespace()}");
            this.Pod = await this.Kubernetes.CoreV1.CreateNamespacedPodAsync(this.Pod, this.Pod.Namespace(), cancellationToken: cancellationToken);
            this.Logger.LogDebug("The pod '{pod}' has been successfully created", $"{this.Pod.Name()}.{this.Pod.Namespace()}");
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while creating the specified pod '{pod}': {ex}", $"{this.Pod.Name()}.{this.Pod.Namespace()}", ex);
        }
        _ = Task.Run(() => this.ReadPodLogsAsync(cancellationToken), cancellationToken);
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
        using var reader = new StreamReader(logStream);
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null) this.StandardOutputSubject.OnNext(line);
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
        var response = this.Kubernetes.CoreV1.ListNamespacedPodWithHttpMessagesAsync(this.Pod.Namespace(), fieldSelector: $"metadata.name={Pod.Name()}", watch: true, cancellationToken: cancellationToken);
        await foreach (var (_, item) in response.WatchAsync<V1Pod, V1PodList>(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (item.Status.Phase != "Succeeded" && item.Status.Phase != "Failed") continue;
            var containerStatus = item.Status.ContainerStatuses.FirstOrDefault();
            this._exitCode = containerStatus?.State.Terminated?.ExitCode ?? -1;
            break;
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.Kubernetes.CoreV1.DeleteNamespacedPodAsync(this.Pod.Name(), this.Pod.Namespace(), cancellationToken: cancellationToken).ConfigureAwait(false);
        await this.CancellationTokenSource.CancelAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="KubernetesWorkflowProcess"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="KubernetesWorkflowProcess"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        this.CancellationTokenSource.Dispose();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="KubernetesWorkflowProcess"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="KubernetesWorkflowProcess"/> is being disposed of</param>
    protected override void Dispose(bool disposing)
    {
        this.CancellationTokenSource.Dispose();
        base.Dispose(disposing);
    }

}
