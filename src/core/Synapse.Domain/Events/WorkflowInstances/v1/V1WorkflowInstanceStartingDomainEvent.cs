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

using Synapse.Integration.Events.WorkflowInstances;

namespace Synapse.Domain.Events.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowInstance"/> is starting
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowInstanceStartingIntegrationEvent))]
    public class V1WorkflowInstanceStartingDomainEvent
        : DomainEvent<Models.V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceStartingDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceStartingDomainEvent()
        {
            this.RuntimeIdentifier = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceStartingDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> which's execution is starting</param>
        /// <param name="runtimeIdentifier">The string used to uniquely identify the <see cref="V1WorkflowInstance"/>'s runtime</param>
        public V1WorkflowInstanceStartingDomainEvent(string id, string runtimeIdentifier)
            : base(id)
        {
            this.RuntimeIdentifier = runtimeIdentifier;
        }

        /// <summary>
        /// Gets the string used to uniquely identify the <see cref="V1WorkflowInstance"/>'s runtime
        /// </summary>
        public virtual string RuntimeIdentifier { get; protected set; }

    }

}
