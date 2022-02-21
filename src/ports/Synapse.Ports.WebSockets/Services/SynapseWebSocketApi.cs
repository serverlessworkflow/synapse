using Microsoft.AspNetCore.SignalR;
using Synapse.Ports.WebSockets.Client.Services;

namespace Synapse.Ports.WebSockets.Services
{

    /// <summary>
    /// Represents the <see cref="Hub"/> that exposes the application's web socket API
    /// </summary>
    public class SynapseWebSocketApi
        : Hub<ISynapseWebSocketApiClient>, ISynapseWebSocketApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseWebSocketApi"/>
        /// </summary>
        public SynapseWebSocketApi()
        {

        }

    }

}
