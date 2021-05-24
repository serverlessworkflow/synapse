using CloudNative.CloudEvents;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to publish and subscribe to <see cref="CloudEvent"/>s
    /// </summary>
    public interface ICloudEventBus
        : IHostedService, IObservable<CloudEvent>, IDisposable
    {

        /// <summary>
        /// Publishes the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to publish</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default);

    }

}
