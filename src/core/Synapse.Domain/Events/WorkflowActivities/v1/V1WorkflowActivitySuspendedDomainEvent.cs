using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has been suspended
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivitySuspendedIntegrationEvent))]
    public class V1WorkflowActivitySuspendedDomainEvent
        : DomainEvent<V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivitySuspendedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivitySuspendedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivitySuspendedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> which's execution has been suspended</param>
        public V1WorkflowActivitySuspendedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
