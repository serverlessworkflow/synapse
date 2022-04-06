using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has been suspended
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityResumedIntegrationEvent))]
    public class V1WorkflowActivityResumedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityResumedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityResumedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityResumedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> which's execution has been resumed</param>
        public V1WorkflowActivityResumedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
