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

namespace Synapse.Api.Client.Http.Services;

/// <summary>
/// Represents the default HTTP implementation of the <see cref="ICloudEventApiClient"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="options">The service used to access the current <see cref="SynapseHttpApiClientOptions"/></param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class CloudEventHttpApiClient(IServiceProvider serviceProvider, ILogger<CloudEventHttpApiClient> logger, IJsonSerializer jsonSerializer, IOptions<SynapseHttpApiClientOptions> options, HttpClient httpClient)
    : ApiClientBase(serviceProvider, logger, jsonSerializer, options, httpClient), ICloudEventApiClient
{

    const string PathPrefix = "api/v1/events";

    /// <inheritdoc/>
    public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(e);
        var uri = PathPrefix;
        var json = this.JsonSerializer.SerializeToText(e);
        using var content = new StringContent(json, Encoding.UTF8, CloudEventContentType.Json);
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Post, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

}
