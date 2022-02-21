namespace Synapse.Application.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to create <see cref="IIntegrationEventBus"/>es
    /// </summary>
    public interface IIntegrationEventBusFactory
    {

        /// <summary>
        /// Creates a new <see cref="IIntegrationEventBus"/>
        /// </summary>
        /// <returns>A new <see cref="IIntegrationEventBus"/></returns>
        IIntegrationEventBus Create();

    }


}
