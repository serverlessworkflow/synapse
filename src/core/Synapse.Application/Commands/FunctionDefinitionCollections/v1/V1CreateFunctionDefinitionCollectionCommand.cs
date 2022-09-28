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

using Semver;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Application.Queries.FunctionDefinitionCollections;

namespace Synapse.Application.Commands.FunctionDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1FunctionDefinitionCollection"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.FunctionDefinitionCollections.V1CreateFunctionDefinitionCollectionCommand))]
    public class V1CreateFunctionDefinitionCollectionCommand
        : Command<Integration.Models.V1FunctionDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateFunctionDefinitionCollectionCommand"/>
        /// </summary>
        protected V1CreateFunctionDefinitionCollectionCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateFunctionDefinitionCollectionCommand"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="V1FunctionDefinitionCollection"/> to create</param>
        /// <param name="version">The version of the <see cref="V1FunctionDefinitionCollection"/> to create</param>
        /// <param name="description">The description of the <see cref="V1FunctionDefinitionCollection"/> to create</param>
        /// <param name="functions">An <see cref="IEnumerable{T}"/> containing the <see cref="FunctionDefinition"/>s the <see cref="V1FunctionDefinitionCollection"/> to create is made out of</param>
        public V1CreateFunctionDefinitionCollectionCommand(string name, string version, string? description, IEnumerable<FunctionDefinition> functions)
        {
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.Functions = functions.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the name of the <see cref="V1FunctionDefinitionCollection"/> to create
        /// </summary>
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the version of the <see cref="V1FunctionDefinitionCollection"/> to create
        /// </summary>
        public virtual string Version { get; protected set; } = null!;

        /// <summary>
        /// Gets the description of the <see cref="V1FunctionDefinitionCollection"/> to create
        /// </summary>
        public virtual string? Description { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="FunctionDefinition"/>s the <see cref="V1FunctionDefinitionCollection"/> to create is made out of
        /// </summary>
        public virtual IReadOnlyCollection<FunctionDefinition> Functions { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateFunctionDefinitionCollectionCommand"/>s
    /// </summary>
    public class V1CreateFunctionDefinitionCollectionCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateFunctionDefinitionCollectionCommand, Integration.Models.V1FunctionDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="CommandHandlerBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="functionCollections">The <see cref="IRepository"/> used to manage <see cref="V1FunctionDefinitionCollection"/>s</param>
        public V1CreateFunctionDefinitionCollectionCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1FunctionDefinitionCollection> functionCollections) 
            : base(loggerFactory, mediator, mapper)
        {
            this.FunctionCollections = functionCollections;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1FunctionDefinitionCollection"/>s
        /// </summary>
        protected IRepository<V1FunctionDefinitionCollection> FunctionCollections { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1FunctionDefinitionCollection>> HandleAsync(V1CreateFunctionDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
        {
            SemVersion version = null!;
            try
            {
                var collectionDto = await this.Mediator.ExecuteAndUnwrapAsync(V1GetFunctionDefinitionCollectionByIdQuery.Parse(command.Name.ToLowerInvariant().Slugify("-")));
                version = SemVersion.Parse(collectionDto.Version, SemVersionStyles.Any);
            }
            catch {}
            if (version == null)
            {
                version = SemVersion.Parse(command.Version, SemVersionStyles.Any);
            }
            else
            {
                if (SemVersion.ComparePrecedence(SemVersion.Parse(command.Version, SemVersionStyles.Any), version) <= 0)
                    version = version.WithPatch(version.Patch + 1);
            }
            var collection = await this.FunctionCollections.AddAsync(new V1FunctionDefinitionCollection(command.Name, version.ToString(), command.Description, command.Functions.ToArray()), cancellationToken);
            await this.FunctionCollections.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1FunctionDefinitionCollection>(collection));
        }

    }

}
