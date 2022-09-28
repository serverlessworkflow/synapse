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
using Synapse.Domain.Events.FunctionDefinitionCollections;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a managed <see cref="FunctionDefinition"/> collection
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1FunctionDefinitionCollection))]
    public class V1FunctionDefinitionCollection
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1FunctionDefinitionCollection"/>
        /// </summary>
        protected V1FunctionDefinitionCollection()
            : base(null!)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1FunctionDefinitionCollection"/>
        /// </summary>
        /// <param name="name">The <see cref="V1FunctionDefinitionCollection"/>'s name</param>
        /// <param name="version">The <see cref="V1FunctionDefinitionCollection"/>'s version</param>
        /// <param name="description">The <see cref="V1FunctionDefinitionCollection"/>'s description</param>
        /// <param name="functions">An array containg the <see cref="FunctionDefinition"/>s the <see cref="V1FunctionDefinitionCollection"/> is made out of</param>
        public V1FunctionDefinitionCollection(string name, string version, string? description = null, params FunctionDefinition[] functions) 
            : base(BuildId(name, version))
        {
            if(string.IsNullOrEmpty(name)) throw DomainException.ArgumentNullOrWhitespace(nameof(name));
            if (string.IsNullOrEmpty(version))throw DomainException.ArgumentNullOrWhitespace(nameof(version));
            if(functions == null) throw DomainException.ArgumentNull(nameof(functions));
            this.On(this.RegisterEvent(new V1FunctionDefinitionCollectionCreatedDomainEvent(this.Id, name, version, description, functions)));
        }

        /// <summary>
        /// Gets the <see cref="V1FunctionDefinitionCollection"/>'s name
        /// </summary>
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1FunctionDefinitionCollection"/>'s version
        /// </summary>
        public virtual string Version { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1FunctionDefinitionCollection"/>'s description
        /// </summary>
        public virtual string? Description { get; protected set; }

        private List<FunctionDefinition> _Functions = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="FunctionDefinition"/>s the <see cref="V1FunctionDefinitionCollection"/> is made out of
        /// </summary>
        public virtual IReadOnlyCollection<FunctionDefinition> Functions => this._Functions.AsReadOnly();

        /// <summary>
        /// Deletes the <see cref="V1FunctionDefinitionCollection"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1FunctionDefinitionCollectionDeletedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1FunctionDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1FunctionDefinitionCollectionCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1FunctionDefinitionCollectionCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.Name = e.Name;
            this.Version = e.Version;
            this.Description = e.Description;
            this._Functions = e.Functions.ToList();
        }

        /// <summary>
        /// Handles the specified <see cref="V1FunctionDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1FunctionDefinitionCollectionDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1FunctionDefinitionCollectionDeletedDomainEvent e)
        {

        }

        /// <summary>
        /// Builds a new id for the specified <see cref="V1FunctionDefinitionCollection"/> name and version
        /// </summary>
        /// <param name="name">The name of the <see cref="V1FunctionDefinitionCollection"/> to create a new id for</param>
        /// <param name="version">The version of the <see cref="V1FunctionDefinitionCollection"/> to create a new id for</param>
        /// <returns>A new id for the specified <see cref="V1FunctionDefinitionCollection"/> name and version</returns>
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
