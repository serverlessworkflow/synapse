using Synapse.Integration.Models;

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Defines the fundamentals of an <see cref="V1CloudEvent"/> stream
    /// </summary>
    public interface IIntegrationEventStream
        : IObservable<V1CloudEvent>
    {


    }

}
