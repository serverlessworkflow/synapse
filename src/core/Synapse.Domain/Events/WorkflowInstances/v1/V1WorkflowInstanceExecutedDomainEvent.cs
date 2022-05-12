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
    /// Represents the <see cref="IDomainEvent"/> fired whenever a <see cref="V1WorkflowInstance"/> has been executed
    /// </summary>
    [DataTransferObjectType(typeof(V1WorkflowInstanceExecutedIntegrationEvent))]
    public class V1WorkflowInstanceExecutedDomainEvent
        : DomainEvent<Models.V1WorkflowInstance, string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        protected V1WorkflowInstanceExecutedDomainEvent()
        {
            this.Logs = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> that has been executed</param>
        /// <param name="status">The <see cref="V1WorkflowInstanceStatus"/> of the <see cref="V1WorkflowInstance"/> when it finished executing</param>
        /// <param name="logs">The logs associated with the <see cref="V1WorkflowInstance"/>'s execution</param>
        /// <param name="error">The error that has occured during the <see cref="V1WorkflowInstance"/>'s execution</param>
        public V1WorkflowInstanceExecutedDomainEvent(string id, V1WorkflowInstanceStatus status, string logs, Neuroglia.Error? error = null)
            : base(id)
        {
            this.Status = status;
            this.Logs = logs;
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> that has been executed</param>
        /// <param name="status">The <see cref="V1WorkflowInstanceStatus"/> of the <see cref="V1WorkflowInstance"/> when it finished executing</param>
        /// <param name="logs">The logs associated with the <see cref="V1WorkflowInstance"/>'s execution</param>
        /// <param name="output">The <see cref="V1WorkflowInstance"/>'s output</param>
        public V1WorkflowInstanceExecutedDomainEvent(string id, V1WorkflowInstanceStatus status, string logs, object? output)
            : base(id)
        {
            this.Status = status;
            this.Logs = logs;
            this.Output = output;
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceExecutedDomainEvent"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> that has been executed</param>
        /// <param name="status">The <see cref="V1WorkflowInstanceStatus"/> of the <see cref="V1WorkflowInstance"/> when it finished executing</param>
        /// <param name="logs">The logs associated with the <see cref="V1WorkflowInstance"/>'s execution</param>
        public V1WorkflowInstanceExecutedDomainEvent(string id, V1WorkflowInstanceStatus status, string logs)
            : base(id)
        {
            this.Status = status;
            this.Logs = logs;
        }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstanceStatus"/> of the <see cref="V1WorkflowInstance"/> when it finished executing
        /// </summary>
        public virtual V1WorkflowInstanceStatus Status { get; protected set; }

        /// <summary>
        /// Gets the logs associated with the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        public virtual string Logs { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s error, if any
        /// </summary>
        public virtual Neuroglia.Error? Error { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s output, if any
        /// </summary>
        public virtual object? Output { get; protected set; }

    }

}
