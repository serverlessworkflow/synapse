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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuroglia.Eventing.CloudEvents;
using Neuroglia.Serialization;
using Synapse.Api.Application.Configuration;
using System.Text;

namespace Synapse.Api.Application.Commands.Events;

/// <summary>
/// Represents the <see cref="ICommand"/> used to publish a <see cref="Neuroglia.Eventing.CloudEvents.CloudEvent"/> to the configured sink
/// </summary>
/// <param name="e">The <see cref="Neuroglia.Eventing.CloudEvents.CloudEvent"/> to publish</param>
public class PublishCloudEventCommand(CloudEvent e)
    : Command
{

    /// <summary>
    /// Gets the <see cref="Neuroglia.Eventing.CloudEvents.CloudEvent"/> to publish
    /// </summary>
    public virtual CloudEvent CloudEvent { get; } = e;

}

/// <summary>
/// Represents the service used to handle <see cref="PublishCloudEventCommand"/>s
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="options">The service used to access the current <see cref="ApiServerOptions"/></param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class PublishCloudEventCommandHandler(ILogger<PublishCloudEventCommandHandler> logger, IOptions<ApiServerOptions> options, IJsonSerializer jsonSerializer, HttpClient httpClient)
    : ICommandHandler<PublishCloudEventCommand>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult> HandleAsync(PublishCloudEventCommand command, CancellationToken cancellationToken = default)
    {
        if (options.Value.CloudEvents.Endpoint == null)
        {
            logger.LogWarning("No endpoint configured for cloud events. Event will not be published.");
            return this.Ok();
        }
        var json = jsonSerializer.SerializeToText(command.CloudEvent);
        using var content = new StringContent(json, Encoding.UTF8, CloudEventContentType.Json);
        using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.CloudEvents.Endpoint) { Content = content };
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("An error occurred while publishing the cloud event with id '{eventId}' to the configure endpoint '{endpoint}': {ex}", command.CloudEvent.Id, options.Value.CloudEvents.Endpoint, json);
            response.EnsureSuccessStatusCode();
        }
        return this.Ok();
    }

}