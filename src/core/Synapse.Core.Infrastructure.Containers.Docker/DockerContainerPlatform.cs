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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Synapse.Core.Infrastructure.Containers.Configuration;
using Synapse.Core.Infrastructure.Services;

namespace Synapse.Core.Infrastructure.Containers;

/// <summary>
/// Represents the Docker implementation of the <see cref="IContainerPlatform"/> interface
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="hostEnvironment">The current <see cref="IHostEnvironment"/></param>
/// <param name="options">The current <see cref="DockerContainerPlatformOptions"/></param>
public class DockerContainerPlatform(ILogger<DockerContainerPlatform> logger, IHostEnvironment hostEnvironment, IOptions<DockerContainerPlatformOptions> options)
    : IHostedService, IContainerPlatform, IDisposable, IAsyncDisposable
{

    bool _disposed;

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
    protected IDockerClient? Docker { get; set; }

    /// <summary>
    /// Gets the current <see cref="DockerContainerPlatformOptions"/>
    /// </summary>
    protected DockerContainerPlatformOptions Options { get; } = options.Value;

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        var dockerConfiguration = new DockerClientConfiguration(this.Options.Api.Endpoint);
        this.Docker = dockerConfiguration.CreateClient(string.IsNullOrWhiteSpace(this.Options.Api.Version) ? null : System.Version.Parse(this.Options.Api.Version!));
        if (!this.Environment.RunsInDocker()) return;
        var containerShortId = System.Environment.MachineName;
        var containerId = (await this.Docker.Containers.InspectContainerAsync(containerShortId, cancellationToken)).ID;
        var response = null as NetworkResponse;
        try
        {
            response = await this.Docker.Networks.InspectNetworkAsync(this.Options.Network, cancellationToken);
        }
        catch (DockerNetworkNotFoundException)
        {
            await this.Docker.Networks.CreateNetworkAsync(new() { Name = this.Options.Network }, cancellationToken);
        }
        finally
        {
            if (response == null || !response!.Containers.ContainsKey(containerId)) await this.Docker.Networks.ConnectNetworkAsync(this.Options.Network, new NetworkConnectParameters() { Container = containerId }, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IContainer> CreateAsync(ContainerProcessDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (this.Docker == null) throw new NullReferenceException("The DockerContainerPlatform has not been properly initialized");
        try
        {
            await this.Docker.Images.InspectImageAsync(definition.Image, cancellationToken).ConfigureAwait(false);
        }
        catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var downloadProgress = new Progress<JSONMessage>();
            var imageComponents = definition.Image.Split(':');
            var imageName = imageComponents[0];
            var imageTag = imageComponents.Length > 1 ? imageComponents[1] : null;
            await this.Docker.Images.CreateImageAsync(new() { FromImage = imageName, Tag = imageTag }, new(), downloadProgress, cancellationToken).ConfigureAwait(false);
        }
        var parameters = new CreateContainerParameters()
        {
            Image = definition.Image,
            Cmd = string.IsNullOrWhiteSpace(definition.Command) ? null : ["/bin/sh", "-c", definition.Command],
            Env = definition.Environment?.Select(e => $"{e.Key}={e.Value}").ToList(),
            HostConfig = new()
            {
                PortBindings = definition.Ports?.ToDictionary(kvp => kvp.Value.ToString(), kvp => (IList<PortBinding>)[new PortBinding() { HostPort = kvp.Key.ToString() }]),
                Binds = definition.Volumes?.Select(e => $"{e.Key}={e.Value}")?.ToList() ?? []
            }
        };
        var response = await this.Docker.Containers.CreateContainerAsync(parameters, cancellationToken).ConfigureAwait(false);
        if (this.Environment.RunsInDocker()) await this.Docker.Networks.ConnectNetworkAsync(this.Options.Network, new NetworkConnectParameters() { Container = response.ID }, cancellationToken);
        foreach (var warning in response.Warnings)
        {
            this.Logger.LogWarning(warning);
        }
        return new DockerContainer(response.ID, this.Docker);
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Disposes of the <see cref="DockerContainerPlatform"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="DockerContainerPlatform"/> is being disposed of</param>
    /// <returns>A new <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (this._disposed) return;
        if (disposing)
        {
            this.Docker?.Dispose();
        }
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
    /// Disposes of the <see cref="DockerContainerPlatform"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="DockerContainerPlatform"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;
        if (disposing)
        {
            this.Docker?.Dispose();
        }
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
