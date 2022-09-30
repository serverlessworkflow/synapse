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
using Synapse.Application.Queries.AuthenticationDefinitionCollections;

namespace Synapse.Application.Commands.AuthenticationDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1AuthenticationDefinitionCollection"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.AuthenticationDefinitionCollections.V1CreateAuthenticationDefinitionCollectionCommand))]
    public class V1CreateAuthenticationDefinitionCollectionCommand
        : Command<Integration.Models.V1AuthenticationDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateAuthenticationDefinitionCollectionCommand"/>
        /// </summary>
        protected V1CreateAuthenticationDefinitionCollectionCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateAuthenticationDefinitionCollectionCommand"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="V1AuthenticationDefinitionCollection"/> to create</param>
        /// <param name="version">The version of the <see cref="V1AuthenticationDefinitionCollection"/> to create</param>
        /// <param name="description">The description of the <see cref="V1AuthenticationDefinitionCollection"/> to create</param>
        /// <param name="authentications">An <see cref="IEnumerable{T}"/> containing the <see cref="AuthenticationDefinition"/>s the <see cref="V1AuthenticationDefinitionCollection"/> to create is made out of</param>
        public V1CreateAuthenticationDefinitionCollectionCommand(string name, string version, string? description, IEnumerable<AuthenticationDefinition> authentications)
        {
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.Authentications = authentications.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the name of the <see cref="V1AuthenticationDefinitionCollection"/> to create
        /// </summary>
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the version of the <see cref="V1AuthenticationDefinitionCollection"/> to create
        /// </summary>
        public virtual string Version { get; protected set; } = null!;

        /// <summary>
        /// Gets the description of the <see cref="V1AuthenticationDefinitionCollection"/> to create
        /// </summary>
        public virtual string? Description { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="AuthenticationDefinition"/>s the <see cref="V1AuthenticationDefinitionCollection"/> to create is made out of
        /// </summary>
        public virtual IReadOnlyCollection<AuthenticationDefinition> Authentications { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateAuthenticationDefinitionCollectionCommand"/>s
    /// </summary>
    public class V1CreateAuthenticationDefinitionCollectionCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateAuthenticationDefinitionCollectionCommand, Integration.Models.V1AuthenticationDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="CommandHandlerBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="authenticationCollections">The <see cref="IRepository"/> used to manage <see cref="V1AuthenticationDefinitionCollection"/>s</param>
        public V1CreateAuthenticationDefinitionCollectionCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1AuthenticationDefinitionCollection> authenticationCollections) 
            : base(loggerFactory, mediator, mapper)
        {
            this.AuthenticationCollections = authenticationCollections;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1AuthenticationDefinitionCollection"/>s
        /// </summary>
        protected IRepository<V1AuthenticationDefinitionCollection> AuthenticationCollections { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1AuthenticationDefinitionCollection>> HandleAsync(V1CreateAuthenticationDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
        {
            SemVersion version = null!;
            try
            {
                var collectionDto = await this.Mediator.ExecuteAndUnwrapAsync(V1GetAuthenticationDefinitionCollectionByIdQuery.Parse(command.Name.ToLowerInvariant().Slugify("-")));
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
            var collection = await this.AuthenticationCollections.AddAsync(new V1AuthenticationDefinitionCollection(command.Name, version.ToString(), command.Description, command.Authentications.ToArray()), cancellationToken);
            await this.AuthenticationCollections.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1AuthenticationDefinitionCollection>(collection));
        }

    }

}
