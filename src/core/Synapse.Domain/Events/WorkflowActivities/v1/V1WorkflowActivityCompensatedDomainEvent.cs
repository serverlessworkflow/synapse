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

using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Domain.Events.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowActivity"/> has been compensated
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityCompensatedIntegrationEvent))]
    public class V1WorkflowActivityCompensatedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCompensatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityCompensatedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCompensatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> which has been compensated</param>
        public V1WorkflowActivityCompensatedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
