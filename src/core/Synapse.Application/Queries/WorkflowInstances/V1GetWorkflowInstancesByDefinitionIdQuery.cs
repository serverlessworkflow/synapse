namespace Synapse.Application.Queries.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get the <see cref="V1WorkflowInstance"/>s of a specific <see cref="V1Workflow"/>
    /// </summary>
    public class V1GetWorkflowInstancesByDefinitionIdQuery
        : Query<List<Integration.Models.V1WorkflowInstance>>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowInstancesByDefinitionIdQuery"/>
        /// </summary>
        protected V1GetWorkflowInstancesByDefinitionIdQuery()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowInstancesByDefinitionIdQuery"/>
        /// </summary>
        /// <param name="workflowId"></param>
        public V1GetWorkflowInstancesByDefinitionIdQuery(string workflowId)
        {
            this.WorkflowId = workflowId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to get the <see cref="V1WorkflowInstance"/>s of
        /// </summary>
        public virtual string WorkflowId { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetWorkflowInstancesByDefinitionIdQuery"/> instances
    /// </summary>
    public class V1GetWorkflowInstancesByDefinitionIdQueryHandler
        : QueryHandlerBase<Integration.Models.V1WorkflowInstance, string>,
        IQueryHandler<V1GetWorkflowInstancesByDefinitionIdQuery, List<Integration.Models.V1WorkflowInstance>>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowInstancesByDefinitionIdQueryHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="repository">The <see cref="IRepository"/> used to manage the entities to query</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1Workflow"/>s</param>
        public V1GetWorkflowInstancesByDefinitionIdQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Integration.Models.V1WorkflowInstance, string> repository, IRepository<Integration.Models.V1Workflow> workflows)
            : base(loggerFactory, mediator, mapper, repository)
        {
            this.Workflows = workflows;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1Workflow"/>es
        /// </summary>
        protected IRepository<Integration.Models.V1Workflow> Workflows { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<List<Integration.Models.V1WorkflowInstance>>> HandleAsync(V1GetWorkflowInstancesByDefinitionIdQuery query, CancellationToken cancellationToken = default)
        {
            var workflow = await this.Workflows.FindAsync(query.WorkflowId, cancellationToken);
            if (workflow == null)
                throw DomainException.NullReference(typeof(V1Workflow), query.WorkflowId);
            return this.Ok(this.Repository.AsQueryable()
                .Where(wf => wf.WorkflowId == workflow.Id)
                .ToList());
        }

    }
}
