using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the metadata of a <see cref="Integration.Models.V1WorkflowActivity"/> has changed
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityMetadataChangedIntegrationEvent))]
    public class V1WorkflowActivityMetadataChangedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityMetadataChangedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityMetadataChangedDomainEvent()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityMetadataChangedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> which's metadata has changed</param>
        /// <param name="metadata">The <see cref="V1WorkflowActivity"/>'s metadata</param>
        public V1WorkflowActivityMetadataChangedDomainEvent(string id, IDictionary<string, string>? metadata)
            : base(id)
        {
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s metadata
        /// </summary>
        public virtual IDictionary<string, string>? Metadata { get; protected set; }

    }

}
