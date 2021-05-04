using k8s.Models;
using Newtonsoft.Json;
using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has been deployed
    /// </summary>
    public class V1WorkflowInstanceDeployedDomainEvent
          : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceDeployedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceDeployedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceDeployedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> has been deployed</param>
        /// <param name="pod">The <see cref="V1ObjectReference"/> of the pod the <see cref="V1WorkflowInstance"/> has been deployed to</param>
        public V1WorkflowInstanceDeployedDomainEvent(string workflowId, V1ObjectReference pod)
        {
            this.AggregateId = workflowId;
            this.Pod = pod;
        }

        /// <summary>
        /// Gets the <see cref="V1ObjectReference"/> of the pod the <see cref="V1WorkflowInstance"/> has been deployed to
        /// </summary>
        [JsonProperty("pod")]
        public V1ObjectReference Pod { get; protected set; }

    }

}
