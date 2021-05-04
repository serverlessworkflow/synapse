using Synapse.Domain.Models;
using System.Collections.Generic;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has faulted
    /// </summary>
    public class V1WorkflowInstanceFaultedDomainEvent
         : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceFaultedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceFaultedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceFaultedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has faulted</param>
        /// <param name="errors">An <see cref="IEnumerable{T}"/> containing the <see cref="V1Error"/>s that have occured during the <see cref="V1WorkflowInstance"/>'s execution</param>
        public V1WorkflowInstanceFaultedDomainEvent(string workflowId, IEnumerable<V1Error> errors)
        {
            this.AggregateId = workflowId;
            this.Errors = errors;
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the <see cref="V1Error"/>s that have occured during the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public IEnumerable<V1Error> Errors { get; protected set; }

    }

}
