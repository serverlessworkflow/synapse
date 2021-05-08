using Synapse.Domain.Models;
using System;

namespace Synapse.Domain.Events.Triggers
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1CorrelationContext"/> has been released by a <see cref="V1Trigger"/>
    /// </summary>
    public class V1TriggerCorrelationContextReleasedDomainEvent
        : DomainEvent<V1Trigger>
    {

        /// <summary>
        /// Initializes a new <see cref="V1TriggerCorrelationContextReleasedDomainEvent"/>
        /// </summary>
        protected V1TriggerCorrelationContextReleasedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1TriggerCorrelationContextReleasedDomainEvent"/>
        /// </summary>
        /// <param name="triggerId">The id of the <see cref="V1Trigger"/> that has released the specified <see cref="V1CorrelationContext"/></param>
        /// <param name="correlationContextId">The id of the <see cref="V1CorrelationContext"/> that has been released by the <see cref="V1Trigger"/></param>
        public V1TriggerCorrelationContextReleasedDomainEvent(string triggerId, Guid correlationContextId)
        {
            this.AggregateId = triggerId;
            this.CorrelationContextId = correlationContextId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1CorrelationContext"/> that has been released by the <see cref="V1Trigger"/>
        /// </summary>
        public Guid CorrelationContextId { get; protected set; }

    }

}
