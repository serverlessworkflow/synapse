namespace Synapse.Application.Services
{
    /// <summary>
    /// Represents a delegate-based <see cref="IIntegrationEventBus"/>
    /// </summary>
    public class DelegateIntegrationEventBus
        : IIntegrationEventBus
    {

        /// <summary>
        /// Initializes a new <see cref="DelegateIntegrationEventBus"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="delegateFunction">The delegate <see cref="Func{T1, T2, T3, TResult}"/> to use</param>
        public DelegateIntegrationEventBus(IServiceProvider serviceProvider, Func<IServiceProvider, CloudEvent, CancellationToken, Task> delegateFunction)
        {
            this.ServiceProvider = serviceProvider;
            this.DelegateFunction = delegateFunction;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the delegate <see cref="Func{T1, T2, T3, TResult}"/> to use
        /// </summary>
        protected Func<IServiceProvider, CloudEvent, CancellationToken, Task> DelegateFunction { get; }

        /// <inheritdoc/>
        public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
        {
            await this.DelegateFunction(this.ServiceProvider, e, cancellationToken);
        }

    }

}
