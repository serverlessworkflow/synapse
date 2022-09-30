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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="Integration.Models.V1EventDefinitionCollection"/> has been deleted
    /// </summary>
    [DataTransferObjectType(typeof(V1EventDefinitionCollectionDeletedIntegrationEvent))]
    public class V1EventDefinitionCollectionDeletedDomainEvent
        : DomainEvent<Models.V1EventDefinitionCollection, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1EventDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        protected V1EventDefinitionCollectionDeletedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1EventDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the deleted <see cref="V1EventDefinitionCollection"/></param>
        public V1EventDefinitionCollectionDeletedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
