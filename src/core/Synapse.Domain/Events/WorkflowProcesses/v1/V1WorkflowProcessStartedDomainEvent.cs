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

namespace Synapse.Domain.Events.V1WorkflowProcesses
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowProcess"/> has started
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Events.WorkflowProcesses.V1WorkflowProcessStartedIntegrationEvent))]
    public class V1WorkflowProcessStartedDomainEvent
        : DomainEvent<Models.V1WorkflowProcess, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessStartedDomainEvent"/>
        /// </summary>
        protected V1WorkflowProcessStartedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessStartedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly started <see cref="V1WorkflowProcess"/></param>
        public V1WorkflowProcessStartedDomainEvent(string id)
            : base(id)
        {

        }

    }

}
