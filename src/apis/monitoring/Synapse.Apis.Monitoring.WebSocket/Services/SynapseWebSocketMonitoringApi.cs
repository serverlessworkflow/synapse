using Microsoft.AspNetCore.SignalR;

namespace Synapse.Apis.Monitoring.WebSocket
{

    /// <summary>
    /// Represents the WebSocket implementation of the <see cref="ISynapseMonitoringApi"/> interface
    /// </summary>
    public class SynapseWebSocketMonitoringApi
        : Hub<ISynapseMonitoringApiClient>, ISynapseMonitoringApi
    {



    }

}
