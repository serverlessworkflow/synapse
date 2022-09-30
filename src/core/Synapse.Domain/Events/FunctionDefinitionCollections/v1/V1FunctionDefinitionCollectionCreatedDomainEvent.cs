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

using Synapse.Integration.Events.FunctionDefinitionCollections;

namespace Synapse.Domain.Events.FunctionDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="Integration.Models.V1FunctionDefinitionCollection"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1FunctionDefinitionCollectionCreatedIntegrationEvent))]
    public class V1FunctionDefinitionCollectionCreatedDomainEvent
        : DomainEvent<Models.V1FunctionDefinitionCollection, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1FunctionDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        protected V1FunctionDefinitionCollectionCreatedDomainEvent()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="V1FunctionDefinitionCollectionCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1FunctionDefinitionCollection"/></param>
        /// <param name="name">The <see cref="V1FunctionDefinitionCollection"/>'s name</param>
        /// <param name="version">The <see cref="V1FunctionDefinitionCollection"/>'s version</param>
        /// <param name="description">The <see cref="V1FunctionDefinitionCollection"/>'s description</param>
        /// <param name="functions">An <see cref="IEnumerable{T}"/> containing the <see cref="FunctionDefinition"/>s the <see cref="V1FunctionDefinitionCollection"/> is made out of</param>
        public V1FunctionDefinitionCollectionCreatedDomainEvent(string id, string name, string version, string? description, IEnumerable<FunctionDefinition> functions)
            : base(id)
        {
            this.Name = name;
            this.Version = version;
            this.Description = description;
            this.Functions = functions.ToList().AsReadOnly();
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

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> containing the <see cref="FunctionDefinition"/>s the <see cref="V1FunctionDefinitionCollection"/> is made out of
        /// </summary>
        public virtual IReadOnlyCollection<FunctionDefinition> Functions { get; protected set; } = null!;

    }

}
