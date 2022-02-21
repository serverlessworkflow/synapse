using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to complete and set the output of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1SetWorkflowInstanceOutputCommandDto))]
    public class V1SetWorkflowInstanceOutputCommand
        : Command<V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceOutputCommand"/>
        /// </summary>
        protected V1SetWorkflowInstanceOutputCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceOutputCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to start</param>
        /// <param name="output">The <see cref="V1WorkflowInstance"/>'s output</param>
        public V1SetWorkflowInstanceOutputCommand(string id, object? output)
        {
            this.Id = id;
            this.Output = output;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to start
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s output
        /// </summary>
        public virtual object? Output { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SetWorkflowInstanceOutputCommand"/>s
    /// </summary>
    public class V1SetWorkflowInstanceOutputCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SetWorkflowInstanceOutputCommand, V1WorkflowInstanceDto>
    {

        /// <inheritdoc/>
        public V1SetWorkflowInstanceOutputCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowInstanceDto>> HandleAsync(V1SetWorkflowInstanceOutputCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            workflowInstance.SetOutput(command.Output);
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowInstanceDto>(workflowInstance));
        }

    }


}
