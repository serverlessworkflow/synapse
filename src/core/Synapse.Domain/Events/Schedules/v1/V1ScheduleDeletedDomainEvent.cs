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

using Synapse.Integration.Events.Schedules;

namespace Synapse.Domain.Events.Schedules
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1Schedule"/> has been deleted
    /// </summary>
    [DataTransferObjectType(typeof(V1ScheduleDeletedIntegrationEvent))]
    public class V1ScheduleDeletedDomainEvent
        : DomainEvent<Models.V1Schedule, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleDeletedDomainEvent"/>
        /// </summary>
        protected V1ScheduleDeletedDomainEvent() { }

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleDeletedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the deleted <see cref="V1Schedule"/></param>
        public V1ScheduleDeletedDomainEvent(string id) : base(id) { }

    }

}
