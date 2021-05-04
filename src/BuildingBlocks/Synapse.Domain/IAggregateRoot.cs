using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;

namespace Synapse.Domain
{

    public interface IAggregateRoot
    {

        string Id { get; }

        string Version { get; }

        IReadOnlyCollection<IDomainEvent> PendingEvents { get; }

        bool HasPatch { get; }

        bool HasStatusPatch { get; }

        IJsonPatchDocument GetPatch();

        IJsonPatchDocument GetStatusPatch();

        void ReplayEvents(params IDomainEvent[] events);

        void ClearPendingEvents();

    }

}
