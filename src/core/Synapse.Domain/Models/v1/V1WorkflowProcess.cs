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

using Synapse.Domain.Events.V1WorkflowProcesses;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a process
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1WorkflowProcess))]
    public class V1WorkflowProcess
        : AggregateRoot<string>
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcess"/>
        /// </summary>
        protected V1WorkflowProcess()
            : base(default!)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowProcess"/>
        /// </summary>
        /// <param name="id">The <see cref="V1WorkflowProcess"/>'s id</param>
        public V1WorkflowProcess(string id)
            : base(id)
        {
            this.On(this.RegisterEvent(new V1WorkflowProcessStartedDomainEvent(this.Id)));
        }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowProcess"/> has exited
        /// </summary>
        public virtual DateTimeOffset? ExitedAt { get; protected set; }

        /// <summary>
        /// Gets the logs associated to the <see cref="V1WorkflowProcess"/>
        /// </summary>
        public virtual string? Logs { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowProcess"/>'s exit code
        /// </summary>
        public virtual long? ExitCode { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="V1WorkflowProcess"/> is running
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual bool IsRunning => !this.ExitedAt.HasValue;

        /// <summary>
        /// Gets the <see cref="V1WorkflowRuntimeSession"/>'s duration
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual TimeSpan? Duration => this.ExitedAt.HasValue ? this.ExitedAt.Value.Subtract(this.CreatedAt) : null;

        /// <summary>
        /// Appends the specified value to the <see cref="V1WorkflowRuntimeSession"/>'s logs
        /// </summary>
        /// <param name="log">The value to append to the <see cref="V1WorkflowRuntimeSession"/>'s logs</param>
        public virtual void AppendLog(string log)
        {
            this.On(this.RegisterEvent(new V1WorkflowProcessOutputtedLogDomainEvent(this.Id, log)));
        }

        /// <summary>
        /// Marks the <see cref="V1WorkflowRuntimeSession"/> as exited
        /// </summary>
        /// <param name="exitCode">The <see cref="V1WorkflowRuntimeSession"/>'s exit code</param>
        public virtual void Exit(long exitCode)
        {
            this.On(this.RegisterEvent(new V1WorkflowProcessExitedDomainEvent(this.Id, exitCode)));
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowProcessStartedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowProcessStartedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowProcessStartedDomainEvent e)
        {
            this.Id = e.AggregateId;
            this.CreatedAt = e.CreatedAt;
            this.LastModified = e.CreatedAt;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowProcessOutputtedLogDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowProcessOutputtedLogDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowProcessOutputtedLogDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.Logs += e.Log;
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowProcessExitedDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowProcessExitedDomainEvent"/> to handle</param>
        protected virtual void On(V1WorkflowProcessExitedDomainEvent e)
        {
            this.LastModified = e.CreatedAt;
            this.ExitedAt = e.CreatedAt;
            this.ExitCode = e.ProcessExitCode;
        }

    }

}
