using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> that has awakened
    /// </summary>
    public class V1WorkflowInstanceAwakenedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceAwakenedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceAwakenedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceAwakenedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has awakened</param>
        public V1WorkflowInstanceAwakenedDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

    

}
