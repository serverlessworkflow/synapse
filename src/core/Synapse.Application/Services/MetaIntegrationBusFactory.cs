namespace Synapse.Application.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IIntegrationEventBusFactory"/>
    /// </summary>
    public class MetaIntegrationBusFactory
        : IIntegrationEventBusFactory
    {

        /// <summary>
        /// Initializes a new <see cref="MetaIntegrationBusFactory"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        public MetaIntegrationBusFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public virtual IIntegrationEventBus Create()
        {
            return ActivatorUtilities.CreateInstance<MetaIntegrationEventBus>(this.ServiceProvider); ;
        }
    }


}
