using CloudNative.CloudEvents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Services
{

    public interface ICloudEventBus
        : IObservable<CloudEvent>, IDisposable
    {

        Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default);

    }

}
