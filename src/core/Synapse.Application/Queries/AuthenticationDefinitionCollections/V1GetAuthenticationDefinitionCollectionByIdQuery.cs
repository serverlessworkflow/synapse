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

namespace Synapse.Application.Queries.AuthenticationDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get a <see cref="V1AuthenticationDefinitionCollection"/> by id
    /// </summary>
    public class V1GetAuthenticationDefinitionCollectionByIdQuery
        : Query<Integration.Models.V1AuthenticationDefinitionCollection>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetAuthenticationDefinitionCollectionByIdQuery"/>
        /// </summary>
        protected V1GetAuthenticationDefinitionCollectionByIdQuery()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1GetAuthenticationDefinitionCollectionByIdQuery"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1AuthenticationDefinitionCollection"/> to get. Should NOT include the version component</param>
        /// <param name="version">The version of the <see cref="V1AuthenticationDefinitionCollection"/> to get. Defaults to 'lastest'</param>
        public V1GetAuthenticationDefinitionCollectionByIdQuery(string id, string? version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1AuthenticationDefinitionCollection"/> to get. Should NOT include the version component
        /// </summary>
        public virtual string Id { get; protected set; } = null!;

        /// <summary>
        /// Gets the version of the <see cref="V1AuthenticationDefinitionCollection"/> to get. Defaults to 'latest'
        /// </summary>
        public virtual string? Version { get; protected set; } = null!;

        /// <summary>
        /// Parses the specified reference into a new <see cref="V1GetAuthenticationDefinitionCollectionByIdQuery"/>
        /// </summary>
        /// <param name="reference">The reference to parse</param>
        /// <returns>A new <see cref="V1GetAuthenticationDefinitionCollectionByIdQuery"/></returns>
        public static V1GetAuthenticationDefinitionCollectionByIdQuery Parse(string reference)
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
    /// Represents the service used to handle <see cref="V1GetAuthenticationDefinitionCollectionByIdQuery"/> instances
    /// </summary>
    public class V1GetAuthenticationDefinitionCollectionByIdQueryHandler
        : QueryHandlerBase<Integration.Models.V1AuthenticationDefinitionCollection, string>,
        IQueryHandler<V1GetAuthenticationDefinitionCollectionByIdQuery, Integration.Models.V1AuthenticationDefinitionCollection>
    {

        /// <inheritdoc/>
        public V1GetAuthenticationDefinitionCollectionByIdQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Integration.Models.V1AuthenticationDefinitionCollection, string> repository)
            : base(loggerFactory, mediator, mapper, repository)
        {

        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1AuthenticationDefinitionCollection>> HandleAsync(V1GetAuthenticationDefinitionCollectionByIdQuery query, CancellationToken cancellationToken = default)
        {
            Integration.Models.V1AuthenticationDefinitionCollection? collection;
            if (string.IsNullOrWhiteSpace(query.Version)
                || query.Version == "latest")
            {
                collection = this.Repository.AsQueryable()
                    .Where(fc => fc.Name!.Equals(query.Id, StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .OrderByDescending(fc => SemVersion.Parse(fc.Version, SemVersionStyles.Any))
                    .FirstOrDefault()!;
            }
            else
            {
                collection = await this.Repository.FindAsync($"{query.Id}:{query.Version}", cancellationToken);
            }
            if (collection == null)
                throw DomainException.NullReference(typeof(V1AuthenticationDefinitionCollection), query.Id);
            return this.Ok(collection);
        }

    }

}
