using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has been cancelled
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityCancelledIntegrationEvent))]
    public class V1WorkflowActivityCancelledDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCancelledDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityCancelledDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCancelledDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> which's execution has been cancelled</param>
        public V1WorkflowActivityCancelledDomainEvent(string id)
            : base(id)
        {

        }

    }

}
