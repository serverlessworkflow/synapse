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

namespace Synapse.Domain.Events.V1WorkflowProcesses
{

    /// <summary>
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowProcess"/> has exited
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Events.WorkflowProcesses.V1WorkflowProcessExitedIntegrationEvent))]
    public class V1WorkflowProcessExitedDomainEvent
        : DomainEvent<Models.V1WorkflowProcess, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessExitedDomainEvent"/>
        /// </summary>
        protected V1WorkflowProcessExitedDomainEvent()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcessExitedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowProcess"/> that has exited</param>
        /// <param name="exitCode">The <see cref="V1WorkflowProcess"/>'s exit code</param>
        public V1WorkflowProcessExitedDomainEvent(string id, long exitCode)
            : base(id)
        {
            this.ExitCode = exitCode;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowProcess"/>'s exit code
        /// </summary>
        public virtual long ExitCode { get; protected set; }

    }

}
