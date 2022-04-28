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

using Synapse.Integration.Events.Correlations;

namespace Synapse.Domain.Events.Correlations
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1Correlation"/> has been deleted
    /// </summary>
    [DataTransferObjectType(typeof(V1CorrelationCreatedIntegrationEvent))]
    public class V1CorrelationDeletedDomainEvent
         : DomainEvent<Models.V1Correlation, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationDeletedDomainEvent"/>
        /// </summary>
        protected V1CorrelationDeletedDomainEvent()
        {
   
        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelationDeletedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Correlation"/> that has been deleted</param>
        public V1CorrelationDeletedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
