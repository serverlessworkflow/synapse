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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1Event"/> has been released by a <see cref="V1Correlation"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1CorrelatedEventReleasedIntegrationEvent))]
    public class V1CorrelatedEventReleasedDomainEvent
        : DomainEvent<Models.V1Correlation, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelatedEventReleasedDomainEvent"/>
        /// </summary>
        protected V1CorrelatedEventReleasedDomainEvent() { }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelatedEventReleasedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the performed correlation</param>
        /// <param name="contextId">The id of the <see cref="V1CorrelationContext"/> the <see cref="V1Event"/> has been released from</param>
        /// <param name="eventId">The id of the <see cref="V1Event"/> that has been released</param>
        public V1CorrelatedEventReleasedDomainEvent(string id, string contextId, string eventId)
            : base(id)
        {
            this.ContextId = contextId;
            this.EventId = eventId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1CorrelationContext"/> the <see cref="V1Event"/> has been released from
        /// </summary>
        public virtual string ContextId { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="V1Event"/> that has been released
        /// </summary>
        public virtual string EventId { get; protected set; } = null!;

    }

}
