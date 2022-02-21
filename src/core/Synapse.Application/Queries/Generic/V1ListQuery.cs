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

namespace Synapse.Application.Queries.Generic
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get an <see cref="IQueryable"/> of the entities of the specified type
    /// </summary>
    /// <typeparam name="TEntity">The type of <see cref="IEntity"/>The type of entities to query</typeparam>
    public class V1ListQuery<TEntity>
        : Query<IQueryable<TEntity>>
        where TEntity : class, IIdentifiable
    {



    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ListQuery{TEntity}"/> instances
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query</typeparam>
    public class V1ListQueryHandler<TEntity>
        : QueryHandlerBase<TEntity>,
        IQueryHandler<V1ListQuery<TEntity>, IQueryable<TEntity>>
        where TEntity : class, IIdentifiable
    {

        /// <inheritdoc/>
        public V1ListQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<TEntity> repository) 
            : base(loggerFactory, mediator, mapper, repository)
        {

        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<IQueryable<TEntity>>> HandleAsync(V1ListQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(this.Ok(this.Repository.AsQueryable()));
        }

    }

}
