using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/>'s correlation key has been set or updated
    /// </summary>
    public class V1WorkflowCorrelationKeySetDomainEvent
         : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationKeySetDomainEvent"/>
        /// </summary>
        protected V1WorkflowCorrelationKeySetDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationKeySetDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> for which a correlation key has been set</param>
        /// <param name="key">The correlation key that has been set or updated</param>
        /// <param name="value">The value of the correlation key that has been set or updated</param>
        public V1WorkflowCorrelationKeySetDomainEvent(string workflowId, string key, string value)
        {
            this.AggregateId = workflowId;
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the correlation key that has been set or updated
        /// </summary>
        public string Key { get; protected set; }

        /// <summary>
        /// Gets the value of the correlation key that has been set or updated
        /// </summary>
        public string Value { get; protected set; }

    }

}
