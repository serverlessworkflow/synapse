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

using Docker.DotNet;
using Synapse.Runtime.Services;
using System.Reactive.Subjects;

namespace Synapse.Runtime.Docker.Services;

/// <summary>
/// Represents the Docker implementation of the <see cref="IWorkflowProcess"/> interface
/// </summary>
/// <param name="docker">The service used to interact with the Docker API</param>
/// <param name="id">The id of the Docker container associated to the process</param>
public class DockerWorkflowProcess(IDockerClient docker, string id)
    : WorkflowProcessBase
{

    long? _exitCode;

    /// <inheritdoc/>
    public override string Id { get; } = id;

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected virtual IDockerClient Docker { get; } = docker;

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
    public override long? ExitCode { get; }

    /// <summary>
    /// Gets the <see cref="DockerWorkflowProcess"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await this.Docker.Containers.StartContainerAsync(this.Id, new() { }, cancellationToken).ConfigureAwait(false);
        _ = this.ReadStandardOutputAsync();
        _ = this.ReadStandardErrorAsync();
    }

    /// <summary>
    /// Reads the container's STDOUT
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ReadStandardOutputAsync()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        using var stream = await this.Docker.Containers.GetContainerLogsAsync(this.Id, new() { Follow = true, ShowStdout = true, ShowStderr = true, Timestamps = false }, this.CancellationTokenSource.Token).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync(this.CancellationTokenSource.Token)) != null && !this.CancellationTokenSource.IsCancellationRequested) this.StandardOutputSubject.OnNext(line);
    }

    /// <summary>
    /// Reads the container's STDERR
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ReadStandardErrorAsync()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var stream = await this.Docker.Containers.GetContainerLogsAsync(this.Id, new() { Follow = true, ShowStdout = false, ShowStderr = true, Timestamps = false }, this.CancellationTokenSource.Token).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync(this.CancellationTokenSource.Token)) != null && !this.CancellationTokenSource.IsCancellationRequested) this.StandardErrorSubject.OnNext(line);
    }

        /// <inheritdoc/>
    public virtual async Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        var response = await this.Docker.Containers.WaitContainerAsync(this.Id, cancellationToken).ConfigureAwait(false);
        this._exitCode = response.StatusCode;
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.Docker.Containers.StopContainerAsync(this.Id, new() { }, cancellationToken).ConfigureAwait(false);
        await this.CancellationTokenSource.CancelAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="DockerWorkflowProcess"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="DockerWorkflowProcess"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        this.CancellationTokenSource.Dispose();
        this.StandardOutputSubject.Dispose();
        this.StandardErrorSubject.Dispose();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="DockerWorkflowProcess"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="DockerWorkflowProcess"/> is being disposed of</param>
    protected override void Dispose(bool disposing)
    {
        this.CancellationTokenSource.Dispose();
        this.StandardOutputSubject.Dispose();
        this.StandardErrorSubject.Dispose();
        base.Dispose(disposing);
    }

}