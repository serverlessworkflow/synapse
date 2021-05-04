using System;

namespace Synapse.Domain
{

    /// <summary>
    /// Represents the base class for all <see cref="IDomainEvent"/>s
    /// </summary>
    /// <typeparam name="TAggregate">The type of <see cref="IAggregateRoot"/> the <see cref="DomainEvent{TAggregate}"/> applies to</typeparam>
    public abstract class DomainEvent<TAggregate>
        : IDomainEvent<TAggregate>
         where TAggregate : class, IAggregateRoot
    {

        /// <summary>
        /// Initializes a new <see cref="DomainEvent{TAggregate}"/>
        /// </summary>
        protected DomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="DomainEvent{TAggregate}"/>
        /// </summary>
        /// <param name="aggregateId">The id of the <see cref="IAggregateRoot"/> the <see cref="DomainEvent{TAggregate}"/> applies to</param>
        protected DomainEvent(string aggregateId)
        {
            this.AggregateId = aggregateId;
            this.CreatedAt = DateTimeOffset.Now;
        }

        /// <inheritdoc/>
        public string AggregateId { get; protected set; }

        string IDomainEvent.AggregateType
        {
            get
            {
                return typeof(TAggregate).Name;
            }
        }

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; protected set; }

    }

}
