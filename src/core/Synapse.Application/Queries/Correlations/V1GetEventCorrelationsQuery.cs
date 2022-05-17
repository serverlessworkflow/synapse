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

namespace Synapse.Application.Queries.Correlations
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get the <see cref="V1Correlation"/>s matching a given <see cref="V1Event"/>
    /// </summary>
    public class V1GetEventCorrelationsQuery
        : Query<IAsyncEnumerable<V1Correlation>>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetEventCorrelationsQuery"/>
        /// </summary>
        protected V1GetEventCorrelationsQuery()
        {
            this.Event = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GetEventCorrelationsQuery"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to get the matching <see cref="V1Correlation"/>s for</param>
        public V1GetEventCorrelationsQuery(V1Event e)
        {
            this.Event = e;
        }

        /// <summary>
        /// Gets the <see cref="V1Event"/> to get the matching <see cref="V1Correlation"/>s for
        /// </summary>
        public virtual V1Event Event { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetEventCorrelationsQuery"/> instances
    /// </summary>
    public class V1GetEventCorrelationsQueryHandler
        : QueryHandlerBase,
        IQueryHandler<V1GetEventCorrelationsQuery, IAsyncEnumerable<V1Correlation>>
    {

        /// <summary>
        /// Initializes a new <see cref="QueryHandlerBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="correlationWriteModels">The <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s</param>
        /// <param name="correlationReadModels">The <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1Correlation"/>s</param>

        public V1GetEventCorrelationsQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IRepository<V1Correlation> correlationWriteModels, IRepository<Integration.Models.V1Correlation> correlationReadModels) 
            : base(loggerFactory, mediator, mapper)
        {
            this.CorrelationWriteModels = correlationWriteModels;
            this.CorrelationReadModels = correlationReadModels;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s
        /// </summary>
        protected IRepository<V1Correlation> CorrelationWriteModels { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1Correlation"/>s
        /// </summary>
        protected IRepository<Integration.Models.V1Correlation> CorrelationReadModels { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<IAsyncEnumerable<V1Correlation>>> HandleAsync(V1GetEventCorrelationsQuery query, CancellationToken cancellationToken = default)
        {
            IEnumerable<Integration.Models.V1Correlation> correlations = await this.CorrelationReadModels.ToListAsync(cancellationToken);
            var totalTriggerCount = correlations.Count();
            var e = this.Mapper.Map<Integration.Models.V1Event>(query.Event);
            correlations = correlations.Where(c => c.AppliesTo(e));
            return this.Ok(this.GetCorrelationsAsync(correlations.Select(c => c.Id).ToList()));
        }

        /// <summary>
        /// Gets all matching <see cref="V1Correlation"/>s
        /// </summary>
        /// <param name="correlationIds">A <see cref="Lazy{T, TMetadata}"/> containing the ids of the <see cref="V1Correlation"/>s to list</param>
        /// <returns>A new awaitable <see cref="IAsyncEnumerable{T}"/></returns>
        protected virtual async IAsyncEnumerable<V1Correlation> GetCorrelationsAsync(List<string> correlationIds)
        {
            foreach (var correlationId in correlationIds)
            {
                yield return await this.CorrelationWriteModels.FindAsync(correlationId);
            }
        }

    }

}
