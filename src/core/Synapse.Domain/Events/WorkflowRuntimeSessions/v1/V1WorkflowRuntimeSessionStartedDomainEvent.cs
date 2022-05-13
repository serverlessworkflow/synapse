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

namespace Synapse.Domain.Events.WorkflowRuntimeSessions
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowRuntimeSession"/> has been started
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Events.WorkflowRuntimeSessions.V1WorkflowProcessStartedIntegrationEvent))]
    public class V1WorkflowRuntimeSessionStartedDomainEvent
        : DomainEvent<Models.V1WorkflowRuntimeSession, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSessionStartedDomainEvent"/>
        /// </summary>
        protected V1WorkflowRuntimeSessionStartedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSessionStartedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the newly started <see cref="V1WorkflowRuntimeSession"/></param>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to</param>
        /// <param name="processId">The id of the process used to run the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to</param>
        public V1WorkflowRuntimeSessionStartedDomainEvent(string id, string workflowInstanceId, string processId)
            : base(id)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.ProcessId = processId;
        }

        /// <summary>
        /// Gets the id of the process used to run the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the process used to run the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to
        /// </summary>
        public virtual string ProcessId { get; protected set; } = null!;

    }

}
