using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to cancel the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1CancelWorkflowInstanceCommandDto))]
    public class V1CancelWorkflowInstanceCommand
        : Command<V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CancelWorkflowInstanceCommand"/>
        /// </summary>
        protected V1CancelWorkflowInstanceCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CancelWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to cancel</param>
        public V1CancelWorkflowInstanceCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to cancel
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CancelWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1CancelWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CancelWorkflowInstanceCommand, V1WorkflowInstanceDto>
    {

        /// <inheritdoc/>
        public V1CancelWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowInstanceDto>> HandleAsync(V1CancelWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            workflowInstance.Cancel();
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowInstanceDto>(workflowInstance));
        }

    }

}
