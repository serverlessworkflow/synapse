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

namespace Synapse.Application.Commands.Generic
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to delete an existing <see cref="IAggregateRoot"/> by id
    /// </summary>
    /// <typeparam name="TAggregate">The type of the <see cref="IAggregateRoot"/> to delete</typeparam>
    /// <typeparam name="TKey">The type of id used to uniquely identify the <see cref="IAggregateRoot"/> to delete</typeparam>
    public class V1DeleteCommand<TAggregate, TKey>
        : Command
        where TAggregate : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCommand{TAggregate, TKey}"/>
        /// </summary>
        protected V1DeleteCommand()
        {
            this.Id = default!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCommand{TAggregate, TKey}"/>
        /// </summary>
        /// <param name="id">The id of the entity to delete</param>
        public V1DeleteCommand(TKey id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the entity to delete
        /// </summary>
        public virtual TKey Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1DeleteCommand{TEntity, TKey}"/> instances
    /// </summary>
    /// <typeparam name="TAggregate">The type of the <see cref="IAggregateRoot"/> to delete</typeparam>
    /// <typeparam name="TKey">The key used to uniquely identify the <see cref="IAggregateRoot"/> to delete</typeparam>
    public class DeleteCommandHandler<TAggregate, TKey>
        : CommandHandlerBase,
        ICommandHandler<V1DeleteCommand<TAggregate, TKey>>
        where TAggregate : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        public DeleteCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<TAggregate, TKey> repository) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Repository = repository;
        }

        protected IRepository<TAggregate, TKey> Repository { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1DeleteCommand<TAggregate, TKey> command, CancellationToken cancellationToken = default)
        {
            if (!await this.Repository.ContainsAsync(command.Id, cancellationToken))
                throw DomainException.NullReference(typeof(TAggregate), command.Id);
            await this.Repository.RemoveAsync(command.Id, cancellationToken);
            await this.Repository.SaveChangesAsync(cancellationToken);
            return this.Ok();
        }

    }

}
