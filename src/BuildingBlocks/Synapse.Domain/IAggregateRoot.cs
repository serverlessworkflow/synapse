using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;

namespace Synapse.Domain
{

    /// <summary>
    /// Defines the fundamentals of an aggregate root
    /// </summary>
    public interface IAggregateRoot
    {

        /// <summary>
        /// Gets the <see cref="IAggregateRoot"/>'s id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the <see cref="IAggregateRoot"/>'s version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="IAggregateRoot"/>'s pending <see cref="IDomainEvent"/>
        /// </summary>
        IReadOnlyCollection<IDomainEvent> PendingEvents { get; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="IAggregateRoot"/> has a patch pending
        /// </summary>
        bool HasPatch { get; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="IAggregateRoot"/> has a status patch pending
        /// </summary>
        bool HasStatusPatch { get; }

        /// <summary>
        /// Gets the <see cref="IAggregateRoot"/>'s patch
        /// </summary>
        /// <returns>The <see cref="IAggregateRoot"/>'s patch</returns>
        IJsonPatchDocument GetPatch();

        /// <summary>
        /// Gets the <see cref="IAggregateRoot"/>'s status patch
        /// </summary>
        /// <returns>The <see cref="IAggregateRoot"/>'s status patch</returns>
        IJsonPatchDocument GetStatusPatch();

        /// <summary>
        /// Replays the specified <see cref="IDomainEvent"/>s
        /// </summary>
        /// <param name="events">An array containing the <see cref="IDomainEvent"/>s to replay</param>
        void ReplayEvents(params IDomainEvent[] events);

        /// <summary>
        /// Clears pending <see cref="IDomainEvent"/>s
        /// </summary>
        void ClearPendingEvents();

    }

}
