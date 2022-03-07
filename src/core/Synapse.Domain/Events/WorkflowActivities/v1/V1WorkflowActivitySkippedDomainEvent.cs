using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="Integration.Models.V1WorkflowActivity"/> has been skipped
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivitySkippedIntegrationEvent))]
    public class V1WorkflowActivitySkippedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivitySkippedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivitySkippedDomainEvent()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivitySkippedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the skipped <see cref="V1WorkflowActivity"/></param>
        public V1WorkflowActivitySkippedDomainEvent(string id)
            : base(id)
        {

        }

    }

    

}
