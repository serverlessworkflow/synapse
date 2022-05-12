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

namespace Synapse.Domain.Models
{
    /// <summary>
    /// Represents a <see cref="V1WorkflowInstance"/>'s runtime sessions
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1WorkflowRuntimeSession))]
    public class V1WorkflowRuntimeSession
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSession"/>
        /// </summary>
        protected V1WorkflowRuntimeSession()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowRuntimeSession"/>
        /// </summary>
        /// <param name="startedAt">The date and time at which the <see cref="V1WorkflowRuntimeSession"/> has started</param>
        /// <param name="runtimeIdentifier">The id used to uniquely identify the runtime the session occurs on</param>
        public V1WorkflowRuntimeSession(DateTimeOffset startedAt, string runtimeIdentifier)
        {
            if(string.IsNullOrWhiteSpace(runtimeIdentifier))
                throw new ArgumentNullException(nameof(runtimeIdentifier));
            this.StartedAt = startedAt;
            this.RuntimeIdentifier = runtimeIdentifier;
        }

        /// <summary>
        /// Gets the string used to uniquely identify the runtime the session occurs on
        /// </summary>
        public virtual string RuntimeIdentifier { get; protected set; } = null!;

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowRuntimeSession"/> has started
        /// </summary>
        public virtual DateTimeOffset StartedAt { get; protected set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowRuntimeSession"/> has ended
        /// </summary>
        public virtual DateTimeOffset? EndedAt { get; protected set; }

        /// <summary>
        /// Gets the logs associated to the <see cref="V1WorkflowRuntimeSession"/>
        /// </summary>
        public virtual string? Logs { get; protected set; }

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
        public virtual TimeSpan? Duration => this.EndedAt.HasValue ? this.EndedAt.Value.Subtract(this.StartedAt) : null;

        /// <summary>
        /// Sets the <see cref="V1WorkflowRuntimeSession"/>'s logs
        /// </summary>
        /// <param name="logs">The <see cref="V1WorkflowRuntimeSession"/>'s logs</param>
        public virtual void SetLogs(string logs)
        {
            this.Logs = logs;
        }

        /// <summary>
        /// Sets the date and time at which the <see cref="V1WorkflowRuntimeSession"/> ended
        /// </summary>
        /// <param name="endedAt">The date and time at which the <see cref="V1WorkflowRuntimeSession"/> ended</param>
        public virtual void MarkAsEnded(DateTimeOffset endedAt)
        {
            this.EndedAt = endedAt;
        }

    }

}
