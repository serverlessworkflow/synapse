/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Application.Queries.Workflows
{
    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get a <see cref="V1Workflow"/> by id
    /// </summary>
    public class V1GetWorkflowVersionsQuery
        : Query<List<Integration.Models.V1Workflow>>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowVersionsQuery"/>
        /// </summary>
        protected V1GetWorkflowVersionsQuery()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowVersionsQuery"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to get. Should NOT include the version component</param>
        public V1GetWorkflowVersionsQuery(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to get. Should NOT include the version component
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetWorkflowVersionsQuery"/> instances
    /// </summary>
    public class V1GetWorkflowVersionsQueryHandler
        : QueryHandlerBase<Integration.Models.V1Workflow, string>,
        IQueryHandler<V1GetWorkflowVersionsQuery, List<Integration.Models.V1Workflow>>
    {

        /// <inheritdoc/>
        public V1GetWorkflowVersionsQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Integration.Models.V1Workflow, string> repository)
            : base(loggerFactory, mediator, mapper, repository)
        {

        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<List<Integration.Models.V1Workflow>>> HandleAsync(V1GetWorkflowVersionsQuery query, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(this.Ok(this.Repository.AsQueryable().Where(wf => wf.Definition.Id == query.Id).ToList()));
        }

    }

}
