using Newtonsoft.Json;
using System;

namespace Synapse.Domain.Models
{
    /// <summary>
    /// Represents the suspension of a workflow instance's execution
    /// </summary>
    public class V1ExecutionInterruption
    {

        /// <summary>
        /// Initializes a new <see cref="V1ExecutionInterruption"/>
        /// </summary>
        protected V1ExecutionInterruption()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1ExecutionInterruption"/>
        /// </summary>
        /// <param name="interruptedAt">The UTC date and time at which the execution has been suspended</param>
        public V1ExecutionInterruption(DateTimeOffset interruptedAt)
        {
            this.SuspendedAt = interruptedAt;
        }

        /// <summary>
        /// Gets the UTC date and time at which the execution has been suspended
        /// </summary>
        [JsonProperty("suspendedAt")]
        public virtual DateTimeOffset SuspendedAt { get; protected set; }

        /// <summary>
        /// Gets the UTC date and time at which the execution has been resumed
        /// </summary>
        [JsonProperty("resumedAt")]
        public virtual DateTimeOffset? ResumedAt { get; protected set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the execution has been resumed
        /// </summary>
        [JsonIgnore]
        public bool HasBeenResumed
        {
            get
            {
                return this.ResumedAt.HasValue;
            }
        }

        /// <summary>
        /// Resumes the workflow execution
        /// </summary>
        /// <param name="resumedAt">The UTC date and time at which the execution has been resumed</param>
        public virtual void Resume(DateTimeOffset resumedAt)
        {
            this.ResumedAt = resumedAt;
        }

    }

}
