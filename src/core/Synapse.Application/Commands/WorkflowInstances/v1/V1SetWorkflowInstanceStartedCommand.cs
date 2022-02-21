using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to mark the execution of a <see cref="V1WorkflowInstance"/> as started
    /// </summary>
    [DataTransferObjectType(typeof(V1SetWorkflowInstanceStartedCommandDto))]
    public class V1SetWorkflowInstanceStartedCommand
        : Command<V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceStartedCommand"/>
        /// </summary>
        protected V1SetWorkflowInstanceStartedCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceStartedCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to mark as started</param>
        public V1SetWorkflowInstanceStartedCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to mark as started
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SetWorkflowInstanceStartedCommand"/>s
    /// </summary>
    public class V1SetWorkflowInstanceStartedCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SetWorkflowInstanceStartedCommand, V1WorkflowInstanceDto>
    {

        /// <inheritdoc/>
        public V1SetWorkflowInstanceStartedCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowInstanceDto>> HandleAsync(V1SetWorkflowInstanceStartedCommand command, CancellationToken cancellationToken = default)
        {
            var instance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (instance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            instance.MarkAsRunning();
            instance = await this.WorkflowInstances.UpdateAsync(instance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowInstanceDto>(instance));
        }

    }

}
