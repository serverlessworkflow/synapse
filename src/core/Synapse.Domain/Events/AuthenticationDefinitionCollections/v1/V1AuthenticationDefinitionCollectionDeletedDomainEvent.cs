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

using Synapse.Integration.Events.AuthenticationDefinitionCollections;

namespace Synapse.Domain.Authentications.AuthenticationDefinitionCollections
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="Integration.Models.V1AuthenticationDefinitionCollection"/> has been deleted
    /// </summary>
    [DataTransferObjectType(typeof(V1AuthenticationDefinitionCollectionDeletedIntegrationEvent))]
    public class V1AuthenticationDefinitionCollectionDeletedDomainEvent
        : DomainEvent<Models.V1AuthenticationDefinitionCollection, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1AuthenticationDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        protected V1AuthenticationDefinitionCollectionDeletedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1AuthenticationDefinitionCollectionDeletedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the deleted <see cref="V1AuthenticationDefinitionCollection"/></param>
        public V1AuthenticationDefinitionCollectionDeletedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
