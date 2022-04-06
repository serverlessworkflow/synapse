using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has started
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityStartedIntegrationEvent))]
    public class V1WorkflowActivityStartedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityStartedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityStartedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityStartedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> that has started</param>
        public V1WorkflowActivityStartedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
