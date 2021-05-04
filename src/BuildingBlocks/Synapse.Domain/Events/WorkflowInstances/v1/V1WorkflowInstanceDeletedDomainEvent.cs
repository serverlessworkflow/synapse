using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has been deleted
    /// </summary>
    public class V1WorkflowInstanceDeletedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceDeletedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceDeletedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceDeletedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has been deleted</param>
        public V1WorkflowInstanceDeletedDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
