using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{
    
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to fault the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1FaultWorkflowInstanceCommandDto))]
    public class V1FaultWorkflowInstanceCommand
        : Command<V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1FaultWorkflowInstanceCommand"/>
        /// </summary>
        protected V1FaultWorkflowInstanceCommand()
        {
            this.Id = null!;
            this.Error = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1FaultWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to fault</param>
        /// <param name="error">The <see cref="Error"/> that caused the <see cref="V1WorkflowInstance"/> to fault</param>
        public V1FaultWorkflowInstanceCommand(string id, Error error)
        {
            this.Id = id;
            this.Error = error;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to fault
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Error"/> that caused the <see cref="V1WorkflowInstance"/> to fault
        /// </summary>
        public virtual Error Error { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1FaultWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1FaultWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1FaultWorkflowInstanceCommand, V1WorkflowInstanceDto>
    {

        /// <inheritdoc/>
        public V1FaultWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowInstanceDto>> HandleAsync(V1FaultWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            workflowInstance.Fault(command.Error);
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowInstanceDto>(workflowInstance));
        }

    }


}
