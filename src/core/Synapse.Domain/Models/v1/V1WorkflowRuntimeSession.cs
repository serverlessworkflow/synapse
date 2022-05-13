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

using Synapse.Domain.Events.WorkflowRuntimeSessions;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents one the processes associated to a <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1WorkflowRuntimeSession))]
    public class V1WorkflowRuntimeSession
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSession"/>
        /// </summary>
        protected V1WorkflowRuntimeSession()
            : base(default!)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSession"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to</param>
        /// <param name="processId">The id of the process used to run the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to</param>
        public V1WorkflowRuntimeSession(string workflowInstanceId, string processId)
            : base($"{workflowInstanceId}-{processId}")
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            if (string.IsNullOrWhiteSpace(processId))
                throw new ArgumentNullException(nameof(processId));
            this.On(this.RegisterEvent(new V1WorkflowRuntimeSessionStartedDomainEvent(this.Id, processId, workflowInstanceId)));
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the process used to run the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowRuntimeSession"/> relates to
        /// </summary>
        public virtual string ProcessId { get; protected set; } = null!;

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowRuntimeSession"/> has exited
        /// </summary>
        public virtual DateTimeOffset? EndedAt { get; protected set; }

        /// <summary>
        /// Gets the logs associated to the <see cref="V1WorkflowRuntimeSession"/>
        /// </summary>
        public virtual string? Logs { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowRuntimeSession"/>'s process exit code
        /// </summary>
        public virtual long? ProcessExitCode { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="V1WorkflowRuntimeSession"/> is active
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual bool IsActive => !this.EndedAt.HasValue;

        /// <summary>
        /// Gets the <see cref="V1WorkflowRuntimeSession"/>'s duration
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual TimeSpan? Duration => this.EndedAt.HasValue ? this.EndedAt.Value.Subtract(this.CreatedAt) : null;

        /// <summary>
        /// Appends the specified value to the <see cref="V1WorkflowRuntimeSession"/>'s logs
        /// </summary>
        /// <param name="log">The value to append to the <see cref="V1WorkflowRuntimeSession"/>'s logs</param>
        public virtual void AppendLog(string log)
        {
            this.On(this.RegisterEvent(new V1LogAppendedToWorkflowRuntimeSessionDomainEvent(this.Id, log)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowRuntimeSession"/> as exited
        /// </summary>
        /// <param name="exitCode">The <see cref="V1WorkflowRuntimeSession"/>'s exit code</param>
        public virtual void Exit(long exitCode)
        {
            this.On(this.RegisterEvent(new V1WorkflowRuntimeSessionEndedDomainEvent(this.Id, exitCode)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowRuntimeSessionStartedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowRuntimeSessionStartedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowRuntimeSessionStartedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
            this.WorkflowInstanceId = e.WorkflowInstanceId;
            this.ProcessId = e.ProcessId;
        }

        /// <summary>
        /// Handles the specified <see cref="V1LogAppendedToWorkflowRuntimeSessionDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1LogAppendedToWorkflowRuntimeSessionDomainEvent"/> to handle</param>
        protected virtual void On(V1LogAppendedToWorkflowRuntimeSessionDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Logs += e.Log;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowRuntimeSessionEndedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowRuntimeSessionEndedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowRuntimeSessionEndedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.EndedAt = e.CreatedAt;
            this.ProcessExitCode = e.ProcessExitCode;
        }

    }

}
