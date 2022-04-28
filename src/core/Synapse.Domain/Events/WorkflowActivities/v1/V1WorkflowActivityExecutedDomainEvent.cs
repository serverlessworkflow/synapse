using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has been executed
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityCompletedIntegrationEvent))]
    public class V1WorkflowActivityExecutedDomainEvent
         : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityExecutedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityExecutedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityExecutedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> that has been executed</param>
        /// <param name="status">The <see cref="V1WorkflowActivityStatus"/> of the <see cref="V1WorkflowActivity"/> when it finished executing</param>
        /// <param name="error">The error that has occured during the <see cref="V1WorkflowActivity"/>'s execution</param>
        public V1WorkflowActivityExecutedDomainEvent(string id, V1WorkflowActivityStatus status, Neuroglia.Error? error = null)
            : base(id)
        {
            this.Status = status;
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityExecutedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> that has been executed</param>
        /// <param name="status">The <see cref="V1WorkflowActivityStatus"/> of the <see cref="V1WorkflowActivity"/> when it finished executing</param>
        /// <param name="output">The <see cref="V1WorkflowActivity"/>'s output</param>
        public V1WorkflowActivityExecutedDomainEvent(string id, V1WorkflowActivityStatus status, object? output)
            : base(id)
        {
            this.Status = status;
            this.Output = output;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityExecutedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> that has been executed</param>
        /// <param name="status">The <see cref="V1WorkflowActivityStatus"/> of the <see cref="V1WorkflowActivity"/> when it finished executing</param>
        public V1WorkflowActivityExecutedDomainEvent(string id, V1WorkflowActivityStatus status)
            : base(id)
        {
            this.Status = status;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivityStatus"/> of the <see cref="V1WorkflowActivity"/> when it finished executing
        /// </summary>
        public virtual V1WorkflowActivityStatus Status { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s error, if any
        /// </summary>
        public virtual Neuroglia.Error? Error { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s output, if any
        /// </summary>
        public virtual object? Output { get; protected set; }

    }

}
