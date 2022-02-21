using Synapse.Integration.Events;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IIntegrationEventBus"/> interface
    /// </summary>
    public class MetaIntegrationEventBus
        : IIntegrationEventBus
    {

        /// <summary>
        /// Initializes a new <see cref="MetaIntegrationEventBus"/>
        /// </summary>
        /// <param name="buses">An <see cref="IEnumerable{T}"/> containing all registered <see cref="IIntegrationEvent"/>es</param>
        public MetaIntegrationEventBus(IEnumerable<IIntegrationEventBus> buses)
        {
            this.Buses = buses.Except(new IIntegrationEventBus[] { this });
        }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> containing all registered <see cref="IIntegrationEvent"/>es
        /// </summary>
        protected IEnumerable<IIntegrationEventBus> Buses { get; }

        /// <inheritdoc/>
        public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();
            foreach (var bus in this.Buses)
            {
                tasks.Add(bus.PublishAsync(e, cancellationToken));
            }
            await Task.WhenAll(tasks);
        }

    }

}
