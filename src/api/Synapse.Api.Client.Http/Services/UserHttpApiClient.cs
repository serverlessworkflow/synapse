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

using Neuroglia.Security;

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IUserApiClient"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="options">The service used to access the current <see cref="SynapseHttpApiClientOptions"/></param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class UserHttpApiClient(IServiceProvider serviceProvider, ILogger<UserHttpApiClient> logger, IJsonSerializer jsonSerializer, IOptions<SynapseHttpApiClientOptions> options, HttpClient httpClient)
    : ApiClientBase(serviceProvider, logger, jsonSerializer, options, httpClient), IUserApiClient
{

    /// <inheritdoc/>
    public virtual async Task<UserInfo> GetUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var uri = $"api/v1/users/profile";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<UserInfo>(json)!;
    }

}
