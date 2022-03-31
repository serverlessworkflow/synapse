/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Synapse.Application.Configuration;
using Synapse.Integration.Models;

namespace Synapse.Apis.Monitoring.WebSocket
{

    /// <summary>
    /// Defines extensions for <see cref="ISynapseApplicationBuilder"/>s
    /// </summary>
    public static class ISynapseApplicationBuilderExtensions
    {

        /// <summary>
        /// Configures Synapse to use the Http REST API port
        /// </summary>
        /// <param name="synapse">The <see cref="ISynapseApplicationBuilder"/> to configure</param>
        /// <returns>The configured <see cref="ISynapseApplicationBuilder"/></returns>
        public static ISynapseApplicationBuilder UseWebSocketMonitoringApi(this ISynapseApplicationBuilder synapse)
        {
            synapse.Services.AddSignalR()
                .AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings = new()
                    {
                        ContractResolver = new NonPublicSetterContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore
                    };
                });
            synapse.Services.AddIntegrationEventBus(async (provider, e, cancellationToken) =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                var hub = provider.GetRequiredService<IHubContext<SynapseWebSocketMonitoringApi, ISynapseMonitoringApiClient>>();
                await hub.Clients.All.PublishIntegrationEvent(mapper.Map<V1Event>(e));
            });
            return synapse;
        }

    }

}
