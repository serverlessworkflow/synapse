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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="V1WorkflowInstance"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowInstanceCreatedIntegrationEvent))]
    public class V1WorkflowInstanceCreatedDomainEvent
        : DomainEvent<Models.V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceCreatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceCreatedDomainEvent()
        {
            this.Key = null!;
            this.WorkflowId = null!;
            this.Input = null!;
            this.CorrelationContext = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1WorkflowInstance"/></param>
        /// <param name="workflowId">The id of the instanciated <see cref="V1Workflow"/></param>
        /// <param name="key">The key of the newly created <see cref="V1WorkflowInstance"/></param>
        /// <param name="activationType">The type of the <see cref="V1WorkflowInstance"/>'s activation</param>
        /// <param name="input">The newly created <see cref="V1WorkflowInstance"/>'s input data</param>
        /// <param name="correlationContext">The newly created <see cref="V1WorkflowInstance"/>'s <see cref="V1CorrelationContext"/></param>
        /// <param name="parentId">The id of the newly created <see cref="V1WorkflowInstance"/>'s parent, if any</param>
        public V1WorkflowInstanceCreatedDomainEvent(string id, string workflowId, string key, V1WorkflowInstanceActivationType activationType, object? input, Models.V1CorrelationContext correlationContext, string? parentId)
            : base(id)
        {
            this.Key = key;
            this.WorkflowId = workflowId;
            this.ActivationType = activationType;
            this.Input = input;
            this.CorrelationContext = correlationContext;
            this.ParentId = parentId;
        }

        /// <summary>
        /// Gets the id of the instanciated <see cref="V1Workflow"/>
        /// </summary>
        public virtual string WorkflowId { get; protected set; }

        /// <summary>
        /// Gets the key of the newly created <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual string Key { get; protected set; }

        /// <summary>
        /// Gets the type of the <see cref="V1WorkflowInstance"/>'s activation
        /// </summary>
        public virtual V1WorkflowInstanceActivationType ActivationType { get; protected set; }

        /// <summary>
        /// Gets the newly created <see cref="V1WorkflowInstance"/>'s input data
        /// </summary>
        public virtual object? Input { get; protected set; }

        /// <summary>
        /// Gets the newly created <see cref="V1WorkflowInstance"/>'s <see cref="V1CorrelationContext"/>
        /// </summary>
        public virtual Models.V1CorrelationContext CorrelationContext { get; protected set; }

        /// <summary>
        /// Gets the id of the newly created <see cref="V1WorkflowInstance"/>'s parent, if any
        /// </summary>
        public virtual string? ParentId { get; protected set; }

    }

}
