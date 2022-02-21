using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to start the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1StartWorkflowInstanceCommandDto))]
    public class V1StartWorkflowInstanceCommand
        : Command<V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1StartWorkflowInstanceCommand"/>
        /// </summary>
        protected V1StartWorkflowInstanceCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1StartWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to start</param>
        public V1StartWorkflowInstanceCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to start
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1StartWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1StartWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1StartWorkflowInstanceCommand, V1WorkflowInstanceDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1StartWorkflowInstanceCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        public V1StartWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances, IWorkflowRuntimeHost runtimeHost)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
            this.RuntimeHost = runtimeHost;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntimeHost"/>
        /// </summary>
        protected IWorkflowRuntimeHost RuntimeHost { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowInstanceDto>> HandleAsync(V1StartWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            var runtimeId = await this.RuntimeHost.StartAsync(workflowInstance, cancellationToken); //todo
            workflowInstance.Start();
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowInstanceDto>(workflowInstance));
        }

    }

}
