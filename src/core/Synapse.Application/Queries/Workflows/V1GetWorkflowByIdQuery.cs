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
    public class V1GetWorkflowByIdQuery
        : Query<Integration.Models.V1Workflow>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowByIdQuery"/>
        /// </summary>
        protected V1GetWorkflowByIdQuery()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowByIdQuery"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to get. Should NOT include the version component</param>
        /// <param name="version">The version of the <see cref="V1Workflow"/> to get. Defaults to 'lastest'</param>
        public V1GetWorkflowByIdQuery(string id, string? version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to get. Should NOT include the version component
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

        /// <summary>
        /// Gets the version of the <see cref="V1Workflow"/> to get. Defaults to 'latest'
        /// </summary>
        public virtual string? Version { get; protected set; } = null!;

        /// <summary>
        /// Parses the specified reference into a new <see cref="V1GetWorkflowByIdQuery"/>
        /// </summary>
        /// <param name="reference">The reference to parse</param>
        /// <returns>A new <see cref="V1GetWorkflowByIdQuery"/></returns>
        public static V1GetWorkflowByIdQuery Parse(string reference)
        {
            if (string.IsNullOrEmpty(reference))
                throw new ArgumentNullException(nameof(reference));
            var components = reference.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var id = components.First();
            string? version = null;
            if (components.Length == 2)
                version = components.Last();
            return new(id, version);
        }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetWorkflowByIdQuery"/> instances
    /// </summary>
    public class V1GetWorkflowByIdQueryHandler
        : QueryHandlerBase<Integration.Models.V1Workflow, string>,
        IQueryHandler<V1GetWorkflowByIdQuery, Integration.Models.V1Workflow>
    {

        /// <inheritdoc/>
        public V1GetWorkflowByIdQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Integration.Models.V1Workflow, string> repository) 
            : base(loggerFactory, mediator, mapper, repository)
        {

        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1Workflow>> HandleAsync(V1GetWorkflowByIdQuery query, CancellationToken cancellationToken = default)
        {
            Integration.Models.V1Workflow? workflow;
            if (string.IsNullOrWhiteSpace(query.Version)
                || query.Version == "latest")
            {
                workflow = this.Repository.AsQueryable()
                    .Where(wf => wf.Definition.Id!.Equals(query.Id, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(wf => wf.Definition.Version)
                    .FirstOrDefault()!;
            }
            else
            {
                workflow = await this.Repository.FindAsync($"{query.Id}:{query.Version}", cancellationToken);
            }
            if (workflow == null)
                throw DomainException.NullReference(typeof(V1Workflow), query.Id);
            return this.Ok(workflow);
        }

    }

}
