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
    /// Represents the <see cref="IDomainEvent"/> fired whenever the execution of a <see cref="V1WorkflowActivity"/> has been completed
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowActivityCompletedIntegrationEvent))]
    public class V1WorkflowActivityCompletedDomainEvent
        : DomainEvent<Models.V1WorkflowActivity, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCompletedDomainEvent"/>
        /// </summary>
        protected V1WorkflowActivityCompletedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowActivityCompletedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> that has started</param>
        /// <param name="output">The <see cref="V1WorkflowActivity"/>'s output, if any</param>
        public V1WorkflowActivityCompletedDomainEvent(string id, object? output)
            : base(id)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowActivity"/>'s output, if any
        /// </summary>
        public virtual object? Output { get; protected set; }

    }

}
