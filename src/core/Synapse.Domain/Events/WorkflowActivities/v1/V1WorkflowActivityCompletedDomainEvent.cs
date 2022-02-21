using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has been completed
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityCompletedIntegrationEvent))]
    public class V1WorkflowActivityCompletedDomainEvent
        : DomainEvent<V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCompletedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityCompletedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCompletedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> that has started</param>
        /// <param name="output">The <see cref="V1WorkflowActivity"/>'s output, if any</param>
        public V1WorkflowActivityCompletedDomainEvent(string id, object? output)
            : base(id)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s output, if any
        /// </summary>
        public virtual object? Output { get; protected set; }

    }

}
