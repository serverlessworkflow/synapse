using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has faulted
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityFaultedIntegrationEvent))]
    public class V1WorkflowActivityFaultedDomainEvent
        : DomainEvent<V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityFaultedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityFaultedDomainEvent()
        {
            this.Error = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityFaultedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> which's execution has faulted</param>
        /// <param name="error">The <see cref="Neuroglia.Error"/> due to which the <see cref="V1WorkflowActivity"/> has faulted</param>
        public V1WorkflowActivityFaultedDomainEvent(string id, Error error)
            : base(id)
        {
            this.Error = error;
        }

        /// <summary>
        /// Gets the <see cref="Neuroglia.Error"/> due to which the <see cref="V1WorkflowActivity"/> has faulted<
        /// </summary>
        public virtual Error Error { get; protected set; }

    }

}
