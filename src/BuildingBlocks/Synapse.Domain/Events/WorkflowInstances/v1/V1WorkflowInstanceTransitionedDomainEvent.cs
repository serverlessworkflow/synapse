using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has transitioned to a new <see cref="StateDefinition"/>
    /// </summary>
    public class V1WorkflowInstanceTransitionedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceTransitionedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceTransitionedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceTransitionedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has transitioned to a new <see cref="StateDefinition"/></param>
        /// <param name="stateName">The new <see cref="StateDefinition"/> the <see cref="V1WorkflowInstance"/> has transitioned to</param>
        public V1WorkflowInstanceTransitionedDomainEvent(string workflowId, string stateName)
        {
            this.AggregateId = workflowId;
            this.StateName = stateName;
        }

        /// <summary>
        /// Gets the new <see cref="StateDefinition"/> the <see cref="V1WorkflowInstance"/> has transitioned to
        /// </summary>
        public string StateName { get; protected set; }

    }

}
