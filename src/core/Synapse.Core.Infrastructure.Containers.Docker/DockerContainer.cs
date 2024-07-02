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

using Synapse.Core.Infrastructure.Services;

namespace Synapse.Core.Infrastructure.Containers;

/// <summary>
/// Represents a Docker <see cref="IContainer"/>
/// </summary>
/// <param name="id">The container's ID</param>
/// <param name="dockerClient">The service used to interact with the Docker API</param>
public class DockerContainer(string id, IDockerClient dockerClient)
    : IContainer
{

    bool _disposed;

    /// <summary>
    /// Gets the container's ID
    /// </summary>
    protected virtual string Id { get; } = id;

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected virtual IDockerClient DockerClient { get; } = dockerClient;

    /// <inheritdoc/>
    public virtual StreamReader? StandardOutput { get; protected set; }

    /// <inheritdoc/>
    public virtual StreamReader? StandardError { get; protected set; }

    /// <inheritdoc/>
    public long? ExitCode { get; protected set; }

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await this.DockerClient.Containers.StartContainerAsync(this.Id, new() { }, cancellationToken).ConfigureAwait(false);
#pragma warning disable CS0618 // Type or member is obsolete
        var standardOutputStream = await this.DockerClient.Containers.GetContainerLogsAsync(this.Id, new() { Follow = true, ShowStdout = true, ShowStderr = true, Timestamps = false }, cancellationToken).ConfigureAwait(false);
        var standardErrorStream = await this.DockerClient.Containers.GetContainerLogsAsync(this.Id, new() { Follow = true, ShowStdout = false, ShowStderr = true, Timestamps = false }, cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        this.StandardOutput = new(standardOutputStream);
        this.StandardError = new(standardErrorStream);
    }

    /// <inheritdoc/>
    public virtual async Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        var response = await this.DockerClient.Containers.WaitContainerAsync(this.Id, cancellationToken).ConfigureAwait(false);
        this.ExitCode = response.StatusCode;
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken = default) => this.DockerClient.Containers.StopContainerAsync(this.Id, new() { }, cancellationToken);

    /// <summary>
    /// Disposes of the <see cref="DockerContainer"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="DockerContainer"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed) return;
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
    /// Disposes of the <see cref="DockerContainer"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="DockerContainer"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed) return;
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}