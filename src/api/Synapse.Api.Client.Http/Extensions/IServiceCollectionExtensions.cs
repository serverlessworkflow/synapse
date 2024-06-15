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

namespace Synapse.Api.Client;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures a client for the Synapse HTTP API
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="setup">An <see cref="Action{T}"/> used to configure the <see cref="SynapseHttpApiClientOptions"/> to use</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapseHttpApiClient(this IServiceCollection services, Action<SynapseHttpApiClientOptions> setup)
    {
        services.Configure(setup);
        services.AddHttpClient<ISynapseApiClient, SynapseHttpApiClient>((provider, http) =>
        {
            var apiClientOptions = provider.GetRequiredService<IOptions<SynapseHttpApiClientOptions>>().Value;
            http.BaseAddress = apiClientOptions.BaseAddress;
        });
        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<SynapseHttpApiClientOptions>>().Value;
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(options.BaseAddress, "api/ws/resources/watch"))
                .WithAutomaticReconnect()
                .Build();
            return new ResourceWatchEventHubClient(connection);
        });
        return services;
    }

}