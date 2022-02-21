using Synapse.Integration.Models;
using Synapse.Ports.WebSockets.Client.Models;

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Defines the fundamentals of an <see cref="V1CloudEventDto"/> stream
    /// </summary>
    public interface IIntegrationEventStream
        : IObservable<CloudEventDescriptor>
    {


    }

}
