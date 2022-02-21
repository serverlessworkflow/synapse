namespace Synapse.Application.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to publish <see cref="IIntegrationEvent"/>s
    /// </summary>
    public interface IIntegrationEventBus
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
