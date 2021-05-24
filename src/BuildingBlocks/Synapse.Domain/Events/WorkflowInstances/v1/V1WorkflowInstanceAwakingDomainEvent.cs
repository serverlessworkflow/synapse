using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> is waking up
    /// </summary>
    public class V1WorkflowInstanceAwakingDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceAwakingDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceAwakingDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceAwakingDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that is waking up</param>
        public V1WorkflowInstanceAwakingDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

    

}
