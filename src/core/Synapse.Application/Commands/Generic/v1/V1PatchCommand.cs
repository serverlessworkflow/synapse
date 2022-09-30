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

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Adapters;

namespace Synapse.Application.Commands.Generic
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to patch an existing <see cref="IAggregateRoot"/>
    /// </summary>
    /// <typeparam name="TAggregate">The type of <see cref="IAggregateRoot"/> to patch</typeparam>
    /// <typeparam name="TResult">The type of <see cref="IDataTransferObject"/> to return</typeparam>
    /// <typeparam name="TKey">The type of id used to uniquely identify the <see cref="IAggregateRoot"/> to patch</typeparam>
    public class V1PatchCommand<TAggregate, TResult, TKey>
        : Command<TResult>
        where TAggregate : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// Initializes a new <see cref="V1PatchCommand{TAggregate, TDto, TKey}"/>
        /// </summary>
        protected V1PatchCommand()
        {
            this.Id = default!;
            this.Patch = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1PatchCommand{TAggregate, TDto, TKey}"/>
        /// </summary>
        /// <param name="id">The id of the entity to patch</param>
        /// <param name="patch">The <see cref="JsonPatchDocument{TModel}"/> to apply</param>
        public V1PatchCommand(TKey id, JsonPatchDocument<TAggregate> patch)
        {
            this.Id = id;
            this.Patch = patch;
        }

        /// <summary>
        /// Gets the id of the entity to patch
        /// </summary>
        public virtual TKey Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="JsonPatchDocument{TModel}"/> to apply
        /// </summary>
        public virtual JsonPatchDocument<TAggregate> Patch { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1PatchCommand{TAggregate, TDto, TKey}"/> instances
    /// </summary>
    /// <typeparam name="TAggregate">The type of <see cref="IAggregateRoot"/> to patch</typeparam>
    /// <typeparam name="TResult">The type of <see cref="IDataTransferObject"/> to return</typeparam>
    /// <typeparam name="TKey">The type of key used to uniquely identify the <see cref="IAggregateRoot"/> to patch</typeparam>
    public class PatchCommandHandler<TAggregate, TResult, TKey>
        : CommandHandlerBase,
        ICommandHandler<V1PatchCommand<TAggregate, TResult, TKey>, TResult>
        where TAggregate : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        /// <inheritdoc/>
        public PatchCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<TAggregate, TKey> repository, IObjectAdapter adapter)
            : base(loggerFactory, mediator, mapper)
        {
            this.Repository = repository;
            this.Adapter = adapter;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage the <see cref="IAggregateRoot"/>s of the specified type
        /// </summary>
        protected IRepository<TAggregate, TKey> Repository { get; }

        /// <summary>
        /// Gets the service used to apply JSON patches
        /// </summary>
        protected IObjectAdapter Adapter { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<TResult>> HandleAsync(V1PatchCommand<TAggregate, TResult, TKey> command, CancellationToken cancellationToken)
        {
            var aggregate = await this.Repository.FindAsync(command.Id, cancellationToken);
            if (aggregate == null)
                throw DomainException.NullReference(typeof(TAggregate), command.Id);
            command.Patch.ApplyTo(aggregate, this.Adapter);
            aggregate = await this.Repository.UpdateAsync(aggregate, cancellationToken);
            await this.Repository.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<TResult>(aggregate));
        }

    }

}
