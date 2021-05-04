using Newtonsoft.Json.Linq;
using Synapse.Domain.Models;

namespace Synapse.Domain.Events.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever an existing <see cref="V1WorkflowInstance"/> has been executed
    /// </summary>
    public class V1WorkflowInstanceExecutedDomainEvent
        : DomainEvent<V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceExecutedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        /// <param name="workflowId">The id of the <see cref="V1WorkflowInstance"/> that has been executed</param>
        /// <param name="executionResult">The <see cref="V1WorkflowInstance"/>'s output</param>
        public V1WorkflowInstanceExecutedDomainEvent(string workflowId, JToken output)
        {
            this.AggregateId = workflowId;
            this.Output = output;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s output
        /// </summary>
        public JToken Output { get; protected set; }

    }

}
