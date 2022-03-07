using Synapse.Ports.WebSockets.Client.Models;

namespace Synapse.Ports.WebSockets.Client.Services
{
    /// <summary>
    /// Defines the fundamentals of a Souche web socket API client
    /// </summary>
    public interface ISynapseWebSocketApiClient
    {

        /// <summary>
        /// Handles the specified <see cref="CloudEventDescriptor"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEventDescriptor"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task PublishIntegrationEvent(CloudEventDescriptor e);

    }

}
