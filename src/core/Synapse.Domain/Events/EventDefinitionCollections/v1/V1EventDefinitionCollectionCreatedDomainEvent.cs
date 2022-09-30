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

using Synapse.Integration.Events.EventDefinitionCollections;

namespace Synapse.Domain.Events.EventDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="Integration.Models.V1EventDefinitionCollection"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1EventDefinitionCollectionCreatedIntegrationEvent))]
    public class V1EventDefinitionCollectionCreatedDomainEvent
        : DomainEvent<Models.V1EventDefinitionCollection, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1EventDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        protected V1EventDefinitionCollectionCreatedDomainEvent()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="V1EventDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1EventDefinitionCollection"/></param>
        /// <param name="name">The <see cref="V1EventDefinitionCollection"/>'s name</param>
        /// <param name="version">The <see cref="V1EventDefinitionCollection"/>'s version</param>
        /// <param name="description">The <see cref="V1EventDefinitionCollection"/>'s description</param>
        /// <param name="functions">An <see cref="IEnumerable{T}"/> containing the <see cref="EventDefinition"/>s the <see cref="V1EventDefinitionCollection"/> is made out of</param>
        public V1EventDefinitionCollectionCreatedDomainEvent(string id, string name, string version, string? description, IEnumerable<EventDefinition> functions)
            : base(id)
        {
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.Events = functions.ToList().AsReadOnly();
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

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="EventDefinition"/>s the <see cref="V1EventDefinitionCollection"/> is made out of
        /// </summary>
        public virtual IReadOnlyCollection<EventDefinition> Events { get; protected set; } = null!;

    }

}
