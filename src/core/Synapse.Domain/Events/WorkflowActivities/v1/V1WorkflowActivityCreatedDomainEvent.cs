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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a new <see cref="Integration.Models.V1WorkflowActivity"/> has been created
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityCreatedIntegrationEvent))]
    public class V1WorkflowActivityCreatedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCreatedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityCreatedDomainEvent()
        {
            this.WorkflowInstanceId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCreatedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly created <see cref="V1WorkflowActivity"/></param>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> the newly created <see cref="V1WorkflowActivity"/> belongs to</param>
        /// <param name="type">The type of the newly created <see cref="V1WorkflowActivity"/></param>
        /// <param name="input">The data of the <see cref="V1WorkflowActivity"/>'s data</param>
        /// <param name="metadata">An <see cref="IDictionary{TKey, TValue}"/> that contains the newly created <see cref="V1WorkflowActivity"/>'s metadata</param>
        /// <param name="parentId">The id of the newly created <see cref="V1WorkflowActivity"/>'s parent, if any</param>
        public V1WorkflowActivityCreatedDomainEvent(string id, string workflowInstanceId, V1WorkflowActivityType type, object? input, IDictionary<string, string>? metadata, string? parentId = null)
            : base(id)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.Type = type;
            this.Input = input;
            this.Metadata = metadata;
            this.ParentId = parentId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> the newly created <see cref="V1WorkflowActivity"/> belongs to
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

        /// <summary>
        /// Gets the newly created <see cref="V1WorkflowActivity"/>'s type
        /// </summary>
        public virtual V1WorkflowActivityType Type { get; protected set; }

        /// <summary>
        /// Gets the newly created <see cref="V1WorkflowActivity"/>'s data
        /// </summary>
        public virtual object? Input { get; protected set; }

        /// <summary>
        /// Gets the newly created <see cref="V1WorkflowActivity"/>'s metadata
        /// </summary>
        public virtual IDictionary<string, string>? Metadata { get; protected set; }

        /// <summary>
        /// Gets the id of the newly created <see cref="V1WorkflowActivity"/>'s parent, if any
        /// </summary>
        public virtual string? ParentId { get; protected set; }

    }

}
