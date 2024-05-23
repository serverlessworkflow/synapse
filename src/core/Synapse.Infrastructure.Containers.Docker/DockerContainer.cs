// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

using Synapse.Infrastructure.Services;

namespace Synapse.Infrastructure.Containers;

/// <summary>
/// Represents a Docker <see cref="IContainer"/>
/// </summary>
/// <param name="id">The container's ID</param>
/// <param name="dockerClient">The service used to interact with the Docker API</param>
public class DockerContainer(string id, IDockerClient dockerClient)
    : IContainer
{

    /// <summary>
    /// Gets the container's ID
    /// </summary>
    protected virtual string Id { get; } = id;

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected virtual IDockerClient DockerClient { get; } = dockerClient;

    /// <inheritdoc/>
    public virtual StreamReader? StandardOutput { get; set; }

    /// <inheritdoc/>
    public virtual StreamReader? StandardError { get; set; }

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
    public virtual Task WaitForExitAsync(CancellationToken cancellationToken = default) => this.DockerClient.Containers.WaitContainerAsync(this.Id, cancellationToken);

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken = default) => this.DockerClient.Containers.StopContainerAsync(this.Id, new() { }, cancellationToken);

}