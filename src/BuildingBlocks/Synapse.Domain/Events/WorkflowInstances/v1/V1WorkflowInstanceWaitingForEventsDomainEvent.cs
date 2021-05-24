using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> is waiting for incoming <see cref="CloudNative.CloudEvents.CloudEvent"/>s
    /// </summary>
    public class V1WorkflowInstanceWaitingForEventsDomainEvent
          : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceWaitingForEventsDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceWaitingForEventsDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceWaitingForEventsDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> is waiting for incoming <see cref="CloudNative.CloudEvents.CloudEvent"/>s</param>
        public V1WorkflowInstanceWaitingForEventsDomainEvent(string workflowId)
        {
            this.AggregateId = workflowId;
        }

    }

}
