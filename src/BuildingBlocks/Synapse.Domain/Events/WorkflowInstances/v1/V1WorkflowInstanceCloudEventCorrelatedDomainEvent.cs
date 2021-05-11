using Newtonsoft.Json;
using Synapse.Domain.Models;
using System.Collections.Generic;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has successfully correlated an incoming <see cref="CloudNative.CloudEvents.CloudEvent"/>
    /// </summary>
    public class V1WorkflowInstanceCloudEventCorrelatedDomainEvent
          : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceCloudEventCorrelatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceCloudEventCorrelatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceCloudEventCorrelatedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> has correlated an incoming <see cref="CloudNative.CloudEvents.CloudEvent"/></param>
        /// <param name="e">The <see cref="V1CloudEvent"/> that has been correlated by the <see cref="V1WorkflowInstance"/></param>
        /// <param name="contextAttributes">An <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="CloudNative.CloudEvents.CloudEvent"/></param>
        public V1WorkflowInstanceCloudEventCorrelatedDomainEvent(string workflowId, V1CloudEvent e, IEnumerable<string> contextAttributes)
        {
            this.AggregateId = workflowId;
            this.CloudEvent = e;
            this.ContextAttributes = contextAttributes;
        }

        /// <summary>
        /// Gets the <see cref="V1CloudEvent"/> that has been correlated by the <see cref="V1WorkflowInstance"/>
        /// </summary>
        [JsonProperty("cloudEvent")]
        public V1CloudEvent CloudEvent { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the context attributes used to correlate the specified <see cref="CloudNative.CloudEvents.CloudEvent"/>
        /// </summary>
        public IEnumerable<string> ContextAttributes { get; protected set; }

    }

}
