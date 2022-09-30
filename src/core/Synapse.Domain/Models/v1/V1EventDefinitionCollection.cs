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
using Synapse.Domain.Events.EventDefinitionCollections;

namespace Synapse.Domain.Models
{
    /// <summary>
    /// Represents a managed <see cref="EventDefinition"/> collection
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1EventDefinitionCollection))]
    public class V1EventDefinitionCollection
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1EventDefinitionCollection"/>
        /// </summary>
        protected V1EventDefinitionCollection()
            : base(null!)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1EventDefinitionCollection"/>
        /// </summary>
        /// <param name="name">The <see cref="V1EventDefinitionCollection"/>'s name</param>
        /// <param name="version">The <see cref="V1EventDefinitionCollection"/>'s version</param>
        /// <param name="description">The <see cref="V1EventDefinitionCollection"/>'s description</param>
        /// <param name="events">An array containg the <see cref="EventDefinition"/>s the <see cref="V1EventDefinitionCollection"/> is made out of</param>
        public V1EventDefinitionCollection(string name, string version, string? description = null, params EventDefinition[] events)
            : base(BuildId(name, version))
        {
            if (string.IsNullOrEmpty(name)) throw DomainException.ArgumentNullOrWhitespace(nameof(name));
            if (string.IsNullOrEmpty(version)) throw DomainException.ArgumentNullOrWhitespace(nameof(version));
            if (events == null) throw DomainException.ArgumentNull(nameof(events));
            this.On(this.RegisterEvent(new V1EventDefinitionCollectionCreatedDomainEvent(this.Id, name, version, description, events)));
        }

        /// <summary>
        /// Gets the <see cref="V1EventDefinitionCollection"/>'s name
        /// </summary>
        public virtual string Name { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1EventDefinitionCollection"/>'s version
        /// </summary>
        public virtual string Version { get; protected set; } = null!;

        /// <summary>
        /// Gets the <see cref="V1EventDefinitionCollection"/>'s description
        /// </summary>
        public virtual string? Description { get; protected set; }

        private List<EventDefinition> _Events = new();
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="EventDefinition"/>s the <see cref="V1EventDefinitionCollection"/> is made out of
        /// </summary>
        public virtual IReadOnlyCollection<EventDefinition> Events => this._Events.AsReadOnly();

        /// <summary>
        /// Deletes the <see cref="V1EventDefinitionCollection"/>
        /// </summary>
        public virtual void Delete()
        {
            this.On(this.RegisterEvent(new V1EventDefinitionCollectionDeletedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1EventDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1EventDefinitionCollectionCreatedDomainEvent"/> to handle</param>
        protected virtual void On(V1EventDefinitionCollectionCreatedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.Name = e.Name;
            this.Version = e.Version;
            this.Description = e.Description;
            this._Events = e.Events.ToList();
        }

        /// <summary>
        /// Handles the specified <see cref="V1EventDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1EventDefinitionCollectionDeletedDomainEvent"/> to handle</param>
        protected virtual void On(V1EventDefinitionCollectionDeletedDomainEvent e)
        {

        }

        /// <summary>
        /// Builds a new id for the specified <see cref="V1EventDefinitionCollection"/> name and version
        /// </summary>
        /// <param name="name">The name of the <see cref="V1EventDefinitionCollection"/> to create a new id for</param>
        /// <param name="version">The version of the <see cref="V1EventDefinitionCollection"/> to create a new id for</param>
        /// <returns>A new id for the specified <see cref="V1EventDefinitionCollection"/> name and version</returns>
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
