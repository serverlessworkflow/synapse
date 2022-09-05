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

namespace Synapse.Application.Queries.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to retrieve a <see cref="V1WorkflowInstance"/> logs
    /// </summary>
    public class V1GetWorkflowInstanceLogsQuery
        : Query<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowInstanceLogsQuery"/>
        /// </summary>
        protected V1GetWorkflowInstanceLogsQuery()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowInstanceLogsQuery"/>
        /// </summary>
        /// <param name="id"></param>
        public V1GetWorkflowInstanceLogsQuery(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to get the logs of
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetWorkflowInstanceLogsQuery"/> instances
    /// </summary>
    public class V1GetWorkflowInstanceLogsQueryHandler
        : QueryHandlerBase<Integration.Models.V1WorkflowInstance>,
        IQueryHandler<V1GetWorkflowInstanceLogsQuery, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowInstanceLogsQueryHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="repository">The <see cref="IRepository"/> used to manage the entities to query</param>
        /// <param name="processes">The <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1WorkflowProcess"/>es</param>
        public V1GetWorkflowInstanceLogsQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Integration.Models.V1WorkflowInstance> repository, IRepository<Integration.Models.V1WorkflowProcess> processes) 
            : base(loggerFactory, mediator, mapper, repository)
        {
            this.Processes = processes;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1WorkflowProcess"/>es
        /// </summary>
        protected IRepository<Integration.Models.V1WorkflowProcess> Processes { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<string>> HandleAsync(V1GetWorkflowInstanceLogsQuery query, CancellationToken cancellationToken = default)
        {
            var instance = await this.Repository.FindAsync(query.Id, cancellationToken);
            if (instance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), query.Id);
            if (instance.Sessions == null)
                return this.Ok(string.Empty);
            var processes = new List<Integration.Models.V1WorkflowProcess>(instance.Sessions.Count);
            foreach(var processId in instance.Sessions.Select(s => s.ProcessId))
            {
                var process = await this.Processes.FindAsync(processId, cancellationToken);
                if (process == null)
                    throw DomainException.NullReference(typeof(V1WorkflowProcess), processId);
                processes.Add(process);
            }
            return this.Ok(processes.Select(p => p.Logs).Aggregate((l1, l2) => l1 += l2)!);
        }

    }
}
