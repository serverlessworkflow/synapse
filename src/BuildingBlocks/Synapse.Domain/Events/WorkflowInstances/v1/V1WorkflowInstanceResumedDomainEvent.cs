using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/>'s execution has been resumed
    /// </summary>
    public class V1WorkflowInstanceResumedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceResumedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceResumedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceResumedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has been resumed</param>
        public V1WorkflowInstanceResumedDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
