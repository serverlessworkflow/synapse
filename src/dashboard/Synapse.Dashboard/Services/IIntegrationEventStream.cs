using Microsoft.Extensions.Hosting;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Defines the fundamentals of an <see cref="V1Event"/> stream
    /// </summary>
    public interface IIntegrationEventStream
        : IObservable<V1Event>
    {


    }

}
