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

using Synapse.Integration.Events.Workflows;

namespace Synapse.Domain.Events.Workflows
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1Workflow"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowCreatedIntegrationEvent))]
    public class V1WorkflowCreatedDomainEvent
        : DomainEvent<Models.V1Workflow, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCreatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowCreatedDomainEvent()
        {
            this.Definition = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1Workflow"/></param>
        /// <param name="definition">The newly created <see cref="V1Workflow"/>'s <see cref="WorkflowDefinition"/></param>
        public V1WorkflowCreatedDomainEvent(string id, WorkflowDefinition definition)
            : base(id)
        {
            this.Definition = definition;
        }

        /// <summary>
        /// Gets the newly created <see cref="V1Workflow"/>'s <see cref="WorkflowDefinition"/>
        /// </summary>
        public virtual WorkflowDefinition Definition { get; protected set; }

    }

}
