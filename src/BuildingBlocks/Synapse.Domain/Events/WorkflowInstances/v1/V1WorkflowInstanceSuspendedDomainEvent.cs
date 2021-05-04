using Synapse.Domain.Models;
using System;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has been suspended
    /// </summary>
    public class V1WorkflowInstanceSuspendedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceSuspendedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceSuspendedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceSuspendedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has been executed</param>
        public V1WorkflowInstanceSuspendedDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
