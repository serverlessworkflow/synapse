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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1Schedule"/> has been made obsolete
    /// </summary>
    [DataTransferObjectType(typeof(V1ScheduleObsolitedIntegrationEvent))]
    public class V1ScheduleObsolitedDomainEvent
        : DomainEvent<Models.V1Schedule, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleObsolitedDomainEvent"/>
        /// </summary>
        protected V1ScheduleObsolitedDomainEvent() { }

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleObsolitedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> that has been made obsolete</param>
        public V1ScheduleObsolitedDomainEvent(string id) : base(id) { }

    }

}
