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
    /// Represents the <see cref="IDomainEvent"/> fired whenever the <see cref="V1CorrelationContext"/> of an existing <see cref="V1WorkflowInstance"/> has changed
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowCorrelationContextChangedIntegrationEvent))]
    public class V1WorkflowCorrelationContextChangedDomainEvent
        : DomainEvent<Models.V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationContextChangedDomainEvent"/>
        /// </summary>
        protected V1WorkflowCorrelationContextChangedDomainEvent()
        {
           
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationContextChangedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> that has faulted</param>
        /// <param name="correlationContext">The <see cref="V1WorkflowInstance"/>'s new <see cref="Models.V1CorrelationContext"/></param>
        public V1WorkflowCorrelationContextChangedDomainEvent(string id, Models.V1CorrelationContext correlationContext)
            : base(id)
        {
            this.CorrelationContext = correlationContext;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s new <see cref="Models.V1CorrelationContext"/>
        /// </summary>
        public virtual Models.V1CorrelationContext CorrelationContext { get; protected set; } = null!;

    }

}
