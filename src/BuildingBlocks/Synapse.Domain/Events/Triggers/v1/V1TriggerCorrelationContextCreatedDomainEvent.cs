using Synapse.Domain.Models;

namespace Synapse.Domain.Events.Triggers
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1CorrelationContext"/> for an existing <see cref="V1Trigger"/>
    /// </summary>
    public class V1TriggerCorrelationContextCreatedDomainEvent
        : DomainEvent<V1Trigger>
    {

        /// <summary>
        /// Initializes a new <see cref="V1TriggerCorrelationContextCreatedDomainEvent"/>
        /// </summary>
        protected V1TriggerCorrelationContextCreatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1TriggerCorrelationContextCreatedDomainEvent"/>
        /// </summary>
        /// <param name="triggerId">The id of the <see cref="V1Trigger"/> the specified <see cref="V1CorrelationContext"/> has been created for</param>
        /// <param name="correlationContext">The <see cref="V1Trigger"/>'s newly created <see cref="V1CorrelationContext"/></param>
        public V1TriggerCorrelationContextCreatedDomainEvent(string triggerId, V1CorrelationContext correlationContext)
        {
            this.AggregateId = triggerId;
            this.CorrelationContext = correlationContext;
        }

        /// <summary>
        /// Gets the <see cref="V1Trigger"/>'s newly created <see cref="V1CorrelationContext"/>
        /// </summary>
        public V1CorrelationContext CorrelationContext { get; protected set; }

    }

}
