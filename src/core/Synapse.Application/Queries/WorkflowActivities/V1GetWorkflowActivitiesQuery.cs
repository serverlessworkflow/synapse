using Microsoft.AspNetCore.OData.Query;
using Synapse.Integration.Models;
using Synapse.Integration.Queries.WorkflowActivities;

namespace Synapse.Application.Queries.WorkflowActivities
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get the <see cref="V1WorkflowActivity"/> instances that belong to a specific <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1GetWorkflowActivitiesQueryDto))]
    public class V1GetWorkflowActivitiesQuery
        : Query<List<V1WorkflowActivityDto>>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowActivitiesQuery"/>
        /// </summary>
        protected V1GetWorkflowActivitiesQuery()
        {
            this.WorkflowInstanceId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowActivitiesQuery"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> to get the <see cref="V1WorkflowActivity"/> instances of</param>
        /// <param name="includeNonOperative">A boolean indicating whether or not to include non-operative activities</param>
        /// <param name="parentId">The id of the <see cref="V1WorkflowActivity"/> to get the child activities of</param>
        /// <param name="options">An object used to configure the <see cref="IQuery"/> to execute</param>
        public V1GetWorkflowActivitiesQuery(string workflowInstanceId, bool includeNonOperative = false, string? parentId = null, ODataQueryOptions<V1WorkflowActivityDto>? options = null)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.IncludeNonOperative = includeNonOperative;
            this.ParentId = parentId;
            this.Options = options;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to get the <see cref="V1WorkflowActivity"/> instances of
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not to include non-operative activities
        /// </summary>
        public virtual bool IncludeNonOperative { get; protected set; }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowActivity"/> to get the child activities of
        /// </summary>
        public virtual string? ParentId { get; protected set; }

        /// <summary>
        /// Gets an object used to configure the <see cref="IQuery"/> to execute
        /// </summary>
        public virtual ODataQueryOptions<V1WorkflowActivityDto>? Options { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetWorkflowActivitiesQuery"/> instances
    /// </summary>
    public class V1GetWorkflowActivitiesQueryHandler
        : QueryHandlerBase<V1WorkflowActivityDto>,
        IQueryHandler<V1GetWorkflowActivitiesQuery, List<V1WorkflowActivityDto>>
    {

        /// <inheritdoc/>
        public V1GetWorkflowActivitiesQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowActivityDto> repository) 
            : base(loggerFactory, mediator, mapper, repository)
        {

        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<List<V1WorkflowActivityDto>>> HandleAsync(V1GetWorkflowActivitiesQuery query, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var activities = this.Repository.AsQueryable()
                    .Where(a => a.WorkflowInstanceId == query.WorkflowInstanceId
                    && a.ParentId == query.ParentId);
                if (!query.IncludeNonOperative)
                    activities = activities.Where(a => a.Status < V1WorkflowActivityStatus.Faulted);
                var results = activities as IQueryable;
                if (query.Options != null)
                    results = query.Options.ApplyTo(activities);
                return this.Ok(results.OfType<V1WorkflowActivityDto>().ToList());
            }, cancellationToken);
        }

    }

}
