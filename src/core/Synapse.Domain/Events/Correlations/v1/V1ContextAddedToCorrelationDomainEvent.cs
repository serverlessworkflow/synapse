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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1CorrelationContext"/> has been added to an existing <see cref="V1Correlation"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1ContextAddedToCorrelationIntegrationEvent))]
    public class V1ContextAddedToCorrelationDomainEvent
        : DomainEvent<Models.V1Correlation, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ContextAddedToCorrelationDomainEvent"/>
        /// </summary>
        protected V1ContextAddedToCorrelationDomainEvent()
        {
            this.Context = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1ContextAddedToCorrelationDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of <see cref="V1Correlation"/> a <see cref="V1CorrelationContext"/> has been added to</param>
        /// <param name="context">The <see cref="V1CorrelationContext"/> that has been added to the <see cref="V1Correlation"/></param>
        public V1ContextAddedToCorrelationDomainEvent(string id, Models.V1CorrelationContext context)
            : base(id)
        {
            this.Context = context;
        }

        /// <summary>
        /// Gets the <see cref="V1CorrelationContext"/> that has been added to the <see cref="V1Correlation"/>
        /// </summary>
        public virtual Models.V1CorrelationContext Context { get; protected set; }

    }

}
