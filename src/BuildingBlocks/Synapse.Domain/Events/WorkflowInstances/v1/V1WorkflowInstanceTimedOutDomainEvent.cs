using Synapse.Domain.Models;
using System;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has timed out
    /// </summary>
    public class V1WorkflowInstanceTimedOutDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceTimedOutDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceTimedOutDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceTimedOutDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has timed out</param>
        public V1WorkflowInstanceTimedOutDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
