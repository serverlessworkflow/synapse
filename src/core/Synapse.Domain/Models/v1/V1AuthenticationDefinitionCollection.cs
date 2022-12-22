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
using Synapse.Domain.Authentications.AuthenticationDefinitionCollections;

namespace Synapse.Domain.Models
{
    /// <summary>
    /// Represents a managed <see cref="AuthenticationDefinition"/> collection
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1AuthenticationDefinitionCollection))]
    public class V1AuthenticationDefinitionCollection
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1AuthenticationDefinitionCollection"/>
        /// </summary>
        protected V1AuthenticationDefinitionCollection()
            : base(null!)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1AuthenticationDefinitionCollection"/>
        /// </summary>
        /// <param name="name">The <see cref="V1AuthenticationDefinitionCollection"/>'s name</param>
        /// <param name="version">The <see cref="V1AuthenticationDefinitionCollection"/>'s version</param>
        /// <param name="description">The <see cref="V1AuthenticationDefinitionCollection"/>'s description</param>
        /// <param name="authentications">An array containg the <see cref="AuthenticationDefinition"/>s the <see cref="V1AuthenticationDefinitionCollection"/> is made out of</param>
        public V1AuthenticationDefinitionCollection(string name, string version, string? description = null, params AuthenticationDefinition[] authentications)
            : base(BuildId(name, version))
        {
            if (string.IsNullOrEmpty(name)) throw DomainException.ArgumentNullOrWhitespace(nameof(name));
            if (string.IsNullOrEmpty(version)) throw DomainException.ArgumentNullOrWhitespace(nameof(version));
            if (authentications == null) throw DomainException.ArgumentNull(nameof(authentications));
            this.On(this.RegisterEvent(new V1AuthenticationDefinitionCollectionCreatedDomainEvent(this.Id, name, version, description, authentications)));
        }

        /// <summary>
        /// Gets the <see cref="V1AuthenticationDefinitionCollection"/>'s name
        /// </summary>
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1AuthenticationDefinitionCollection"/>'s version
        /// </summary>
        public virtual string Version { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1AuthenticationDefinitionCollection"/>'s description
        /// </summary>
        public virtual string? Description { get; protected set; }

        [Newtonsoft.Json.JsonProperty(nameof(Authentications))]
        [System.Text.Json.Serialization.JsonPropertyName(nameof(Authentications))]
        private List<AuthenticationDefinition> _Authentications = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="AuthenticationDefinition"/>s the <see cref="V1AuthenticationDefinitionCollection"/> is made out of
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual IReadOnlyCollection<AuthenticationDefinition> Authentications => this._Authentications.AsReadOnly();

        /// <summary>
        /// Deletes the <see cref="V1AuthenticationDefinitionCollection"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1AuthenticationDefinitionCollectionDeletedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1AuthenticationDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1AuthenticationDefinitionCollectionCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1AuthenticationDefinitionCollectionCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.Name = e.Name;
            this.Version = e.Version;
            this.Description = e.Description;
            this._Authentications = e.Authentications.ToList();
        }

        /// <summary>
        /// Handles the specified <see cref="V1AuthenticationDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1AuthenticationDefinitionCollectionDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1AuthenticationDefinitionCollectionDeletedDomainEvent e)
        {

        }

        /// <summary>
        /// Builds a new id for the specified <see cref="V1AuthenticationDefinitionCollection"/> name and version
        /// </summary>
        /// <param name="name">The name of the <see cref="V1AuthenticationDefinitionCollection"/> to create a new id for</param>
        /// <param name="version">The version of the <see cref="V1AuthenticationDefinitionCollection"/> to create a new id for</param>
        /// <returns>A new id for the specified <see cref="V1AuthenticationDefinitionCollection"/> name and version</returns>
        /// <exception cref="DomainArgumentException"></exception>
        public static string BuildId(string name, string version)
        {
            if (string.IsNullOrEmpty(name)) throw DomainException.ArgumentNullOrWhitespace(nameof(name));
            if (string.IsNullOrEmpty(version)) throw DomainException.ArgumentNullOrWhitespace(nameof(version));
            if (!SemVersion.TryParse(version, SemVersionStyles.Any, out _))
                throw new DomainArgumentException($"The specified value '{version}' is not a valid semantic version", nameof(version));
            return $"{name.ToLowerInvariant().Slugify("-")}:{version}";
        }

    }

}
