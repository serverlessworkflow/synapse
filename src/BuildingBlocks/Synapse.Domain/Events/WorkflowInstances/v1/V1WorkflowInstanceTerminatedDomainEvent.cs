using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has been terminated
    /// </summary>
    public class V1WorkflowInstanceTerminatedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceTerminatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceTerminatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceTerminatedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has been terminated</param>
        public V1WorkflowInstanceTerminatedDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
