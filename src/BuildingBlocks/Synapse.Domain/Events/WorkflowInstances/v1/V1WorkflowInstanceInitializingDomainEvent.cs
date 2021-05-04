using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> is being initialized
    /// </summary>
    public class V1WorkflowInstanceInitializingDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceInitializingDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceInitializingDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceInitializingDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that is being initialized</param>
        public V1WorkflowInstanceInitializingDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
