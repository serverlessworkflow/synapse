using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/>'s <see cref="V1CorrelationContext"/> has been updated
    /// </summary>
    public class V1WorkflowCorrelationContextUpdatedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationContextUpdatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowCorrelationContextUpdatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationContextUpdatedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> for which a correlation key has been set</param>
        /// <param name="correlationContext">The updated <see cref="V1CorrelationContext"/></param>
        public V1WorkflowCorrelationContextUpdatedDomainEvent(string workflowId, V1CorrelationContext correlationContext)
        {
            this.AggregateId = workflowId;
            this.CorrelationContext = correlationContext;
        }

        /// <summary>
        /// Gets the updated <see cref="V1CorrelationContext"/>
        /// </summary>
        public V1CorrelationContext CorrelationContext { get; protected set; }

    }

}
