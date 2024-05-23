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
/// Represents the Docker implementation of the <see cref="IContainerPlatform"/> interface
/// </summary>
/// <param name="dockerClient">The service used to interact with the Docker API</param>
public class DockerContainerPlatform(IDockerClient dockerClient)
    : IContainerPlatform
{

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected IDockerClient DockerClient { get; } = dockerClient;

    /// <inheritdoc/>
    public virtual async Task<IContainer> CreateAsync(ContainerProcessDefinition definition, CancellationToken cancellationToken = default)
    {
        try
        {
            await this.DockerClient.Images.InspectImageAsync(definition.Image, cancellationToken).ConfigureAwait(false);
        }
        catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var downloadProgress = new Progress<JSONMessage>();
            await this.DockerClient.Images.CreateImageAsync(new() { FromImage = definition.Image }, new(), downloadProgress, cancellationToken).ConfigureAwait(false);
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
        var response = await this.DockerClient.Containers.CreateContainerAsync(parameters, cancellationToken).ConfigureAwait(false);
        return new DockerContainer(response.ID, this.DockerClient);
    }

}
