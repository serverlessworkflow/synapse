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
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowInstance"/> has faulted
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowInstanceFaultedIntegrationEvent))]
    public class V1WorkflowInstanceFaultedDomainEvent
        : DomainEvent<V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceFaultedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceFaultedDomainEvent()
        {
            this.Error = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceFaultedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> that has faulted</param>
        /// <param name="error">The <see cref="Neuroglia.Error"/> that caused the <see cref="V1WorkflowInstance"/> to fault</param>
        public V1WorkflowInstanceFaultedDomainEvent(string id, Error error)
            : base(id)
        {
            this.Error = error;
        }

        /// <summary>
        /// Gets the <see cref="Neuroglia.Error"/> that caused the <see cref="V1WorkflowInstance"/> to fault
        /// </summary>
        public virtual Error Error { get; protected set; }

    }
}
