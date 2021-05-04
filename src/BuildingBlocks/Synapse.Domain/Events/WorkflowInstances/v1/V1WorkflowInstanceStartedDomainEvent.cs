using Synapse.Domain.Models;
using System;

namespace Synapse.Domain.Events.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> starts
    /// </summary>
    public class V1WorkflowInstanceStartedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceStartedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceStartedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceStartedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the newly created <see cref="V1WorkflowInstance"/></param>
        public V1WorkflowInstanceStartedDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
