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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowRuntimeSession"/> has exited
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Events.WorkflowRuntimeSessions.V1WorkflowProcessExitedIntegrationEvent))]
    public class V1WorkflowRuntimeSessionEndedDomainEvent
        : DomainEvent<Models.V1WorkflowRuntimeSession, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSessionEndedDomainEvent"/>
        /// </summary>
        protected V1WorkflowRuntimeSessionEndedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSessionEndedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowRuntimeSession"/> that has exited</param>
        /// <param name="exitCode">The <see cref="V1WorkflowRuntimeSession"/>'s exit code</param>
        public V1WorkflowRuntimeSessionEndedDomainEvent(string id, long exitCode)
            : base(id)
        {
            this.ProcessExitCode = exitCode;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowRuntimeSession"/>'s exit code
        /// </summary>
        public virtual long ProcessExitCode { get; protected set; }

    }

}
