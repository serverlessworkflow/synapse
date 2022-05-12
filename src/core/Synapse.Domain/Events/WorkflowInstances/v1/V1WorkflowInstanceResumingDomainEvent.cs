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
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowInstance"/> is resuming
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowInstanceResumingIntegrationEvent))]
    public class V1WorkflowInstanceResumingDomainEvent
        : DomainEvent<Models.V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceResumingDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceResumingDomainEvent()
        {
            this.RuntimeIdentifier = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceResumingDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> which's execution is resuming</param>
        /// <param name="runtimeIdentifier">A string used to uniquely identify the runtime that will perform the execution of the resuming <see cref="V1WorkflowInstance"/></param>
        public V1WorkflowInstanceResumingDomainEvent(string id, string runtimeIdentifier)
            : base(id)
        {
            this.RuntimeIdentifier = runtimeIdentifier;
        }

        /// <summary>
        /// Gets a string used to uniquely identify the runtime that will perform the execution of the resuming <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual string RuntimeIdentifier { get; protected set; }

    }

}
