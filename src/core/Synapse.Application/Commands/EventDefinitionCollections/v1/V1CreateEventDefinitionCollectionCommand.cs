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
using Synapse.Application.Queries.EventDefinitionCollections;

namespace Synapse.Application.Commands.EventDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1EventDefinitionCollection"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.EventDefinitionCollections.V1CreateEventDefinitionCollectionCommand))]
    public class V1CreateEventDefinitionCollectionCommand
        : Command<Integration.Models.V1EventDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateEventDefinitionCollectionCommand"/>
        /// </summary>
        protected V1CreateEventDefinitionCollectionCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateEventDefinitionCollectionCommand"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="V1EventDefinitionCollection"/> to create</param>
        /// <param name="version">The version of the <see cref="V1EventDefinitionCollection"/> to create</param>
        /// <param name="description">The description of the <see cref="V1EventDefinitionCollection"/> to create</param>
        /// <param name="events">An <see cref="IEnumerable{T}"/> containing the <see cref="EventDefinition"/>s the <see cref="V1EventDefinitionCollection"/> to create is made out of</param>
        public V1CreateEventDefinitionCollectionCommand(string name, string version, string? description, IEnumerable<EventDefinition> events)
        {
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.Events = events.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the name of the <see cref="V1EventDefinitionCollection"/> to create
        /// </summary>
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the version of the <see cref="V1EventDefinitionCollection"/> to create
        /// </summary>
        public virtual string Version { get; protected set; } = null!;

        /// <summary>
        /// Gets the description of the <see cref="V1EventDefinitionCollection"/> to create
        /// </summary>
        public virtual string? Description { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="EventDefinition"/>s the <see cref="V1EventDefinitionCollection"/> to create is made out of
        /// </summary>
        public virtual IReadOnlyCollection<EventDefinition> Events { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateEventDefinitionCollectionCommand"/>s
    /// </summary>
    public class V1CreateEventDefinitionCollectionCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateEventDefinitionCollectionCommand, Integration.Models.V1EventDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="CommandHandlerBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="eventCollections">The <see cref="IRepository"/> used to manage <see cref="V1EventDefinitionCollection"/>s</param>
        public V1CreateEventDefinitionCollectionCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1EventDefinitionCollection> eventCollections) 
            : base(loggerFactory, mediator, mapper)
        {
            this.EventCollections = eventCollections;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1EventDefinitionCollection"/>s
        /// </summary>
        protected IRepository<V1EventDefinitionCollection> EventCollections { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1EventDefinitionCollection>> HandleAsync(V1CreateEventDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
        {
            SemVersion version = null!;
            try
            {
                var collectionDto = await this.Mediator.ExecuteAndUnwrapAsync(V1GetEventDefinitionCollectionByIdQuery.Parse(command.Name.ToLowerInvariant().Slugify("-")));
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
            var collection = await this.EventCollections.AddAsync(new V1EventDefinitionCollection(command.Name, version.ToString(), command.Description, command.Events.ToArray()), cancellationToken);
            await this.EventCollections.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1EventDefinitionCollection>(collection));
        }

    }

}
