using System;

namespace Synapse.Domain
{

    /// <summary>
    /// Defines the fundamentals of a domain event
    /// </summary>
    public interface IDomainEvent
    {

        /// <summary>
        /// Gets the id of the <see cref="IAggregateRoot"/> the <see cref="IDomainEvent"/> applies to
        /// </summary>
        string AggregateId { get; }

        /// <summary>
        /// Gets the type of the <see cref="IAggregateRoot"/> the <see cref="IDomainEvent"/> applies to
        /// </summary>
        string AggregateType { get; }

        /// <summary>
        /// Gets the date and time at which the <see cref="IDomainEvent"/> has been created
        /// </summary>
        DateTimeOffset CreatedAt { get; }

    }

    /// <summary>
    /// Defines the fundamentals of a domain event
    /// </summary>
    /// <typeparam name="TAggregate">The type of <see cref="IAggregateRoot"/> the <see cref="IDomainEvent{TAggregate}"/> applies to</typeparam>
    public interface IDomainEvent<TAggregate>
        : IDomainEvent
        where TAggregate : class, IAggregateRoot
    {



    }

}
