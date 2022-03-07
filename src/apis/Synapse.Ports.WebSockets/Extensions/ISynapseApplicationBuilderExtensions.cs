using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Synapse.Application.Configuration;
using Synapse.Ports.WebSockets.Client.Models;
using Synapse.Ports.WebSockets.Client.Services;
using Synapse.Ports.WebSockets.Mapping;
using Synapse.Ports.WebSockets.Services;

namespace Synapse.Ports.WebSockets
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
        public static ISynapseApplicationBuilder UseWebSocketApi(this ISynapseApplicationBuilder synapse)
        {
            synapse.AddMappingProfile<MappingProfile>();
            synapse.Services.AddSignalR()
                .AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings = new()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };
                });
            synapse.Services.AddIntegrationEventBus(async (provider, e, cancellationToken) =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                var hub = provider.GetRequiredService<IHubContext<SynapseWebSocketApi, ISynapseWebSocketApiClient>>();
                await hub.Clients.All.PublishIntegrationEvent(mapper.Map<CloudEventDescriptor>(e));
            });
            return synapse;
        }

    }

}
