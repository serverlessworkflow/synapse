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
    /// Represents an <see cref="IQuery"/> used to get an entity by id
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query</typeparam>
    /// <typeparam name="TKey">The type of id used to uniquely identify the entity to get</typeparam>
    public class V1FindByIdQuery<TEntity, TKey>
        : Query<TEntity>
        where TEntity : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// Initializes a new <see cref="V1FindByIdQuery{TEntity, TKey}"/>
        /// </summary>
        protected V1FindByIdQuery()
        {
            this.Id = default!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1FindByIdQuery{TEntity, TKey}"/>
        /// </summary>
        /// <param name="id">The id of the entity to find</param>
        public V1FindByIdQuery(TKey id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the entity to find
        /// </summary>
        public virtual TKey Id { get; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1FindByIdQuery{TEntity, TKey}"/> instances
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to find</typeparam>
    /// <typeparam name="TKey">The type of key used to uniquely identify the entity to find</typeparam>
    public class V1FindByKeyQueryHandler<TEntity, TKey>
        : QueryHandlerBase<TEntity, TKey>,
        IQueryHandler<V1FindByIdQuery<TEntity, TKey>, TEntity>
        where TEntity : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        /// <inheritdoc/>
        public V1FindByKeyQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<TEntity, TKey> repository) 
            : base(loggerFactory, mediator, mapper, repository)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<TEntity>> HandleAsync(V1FindByIdQuery<TEntity, TKey> query, CancellationToken cancellationToken = default)
        {
            var entity = await this.Repository.FindAsync(query.Id, cancellationToken);
            if (entity == null)
                throw DomainException.NullReference(typeof(TEntity), query.Id, nameof(query.Id));
            return this.Ok(entity);
        }

    }

}
