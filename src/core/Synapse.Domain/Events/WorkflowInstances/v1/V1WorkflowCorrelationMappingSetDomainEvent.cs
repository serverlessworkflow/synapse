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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowInstance"/>'s correlation mapping has been set
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowCorrelationMappingSetIntegrationEvent))]
    public class V1WorkflowCorrelationMappingSetDomainEvent
        : DomainEvent<Models.V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationMappingSetDomainEvent"/>
        /// </summary>
        protected V1WorkflowCorrelationMappingSetDomainEvent()
        {
            this.Key = null!;
            this.Value = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowCorrelationMappingSetDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> the correlation mapping that has been set belongs to</param>
        /// <param name="key">The key of the <see cref="V1WorkflowInstance"/>'s correlation mapping that has been set</param>
        /// <param name="value">The value of the <see cref="V1WorkflowInstance"/>'s correlation mapping that has been set</param>
        public V1WorkflowCorrelationMappingSetDomainEvent(string id, string key, string value)
            : base(id)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the key of the <see cref="V1WorkflowInstance"/>'s correlation mapping that has been set
        /// </summary>
        public virtual string Key { get; protected set; }

        /// <summary>
        /// Gets the value of the <see cref="V1WorkflowInstance"/>'s correlation mapping that has been set
        /// </summary>
        public virtual string Value { get; protected set; }

    }

}
