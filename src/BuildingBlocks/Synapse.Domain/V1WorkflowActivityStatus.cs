using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Synapse.Domain.Models;
using System.Runtime.Serialization;

namespace Synapse
{

    /// <summary>
    /// Enumerates all supported values for a <see cref="V1WorkflowActivity"/> status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1WorkflowActivityStatus
    {
        /// <summary>
        /// Indicates that the workflow activity is pending execution
        /// </summary>
        [EnumMember(Value = "PENDING")]
        Pending,
        /// <summary>
        /// Indicates that the workflow activity being initialized
        /// </summary>
        [EnumMember(Value = "INITIALIZING")]
        Initializing,
        /// <summary>
        /// Indicates that the workflow activity being initialized
        /// </summary>
        [EnumMember(Value = "DEPLOYED")]
        Deployed,
        /// <summary>
        /// Indicates that the workflow activity is being executed
        /// </summary>
        [EnumMember(Value = "EXECUTING")]
        Executing,
        /// <summary>
        /// Indicates that the workflow activity's execution has been suspended
        /// </summary>
        [EnumMember(Value = "SUSPENDED")]
        Suspended,
        /// <summary>
        /// Indicates that the workflow activity is waiting to be awakened. Indicates that the activity is in passive correlation mode or is waiting for a specific amount of time (due to a delay state)
        /// </summary>
        [EnumMember(Value = "WAITING")]
        Waiting,
        /// <summary>
        /// Indicates that the workflow activity is awakening
        /// </summary>
        [EnumMember(Value = "AWAKENING")]
        Awakening,
        /// <summary>
        /// Indicates that the workflow activity has been awakened and is pending execution
        /// </summary>
        [EnumMember(Value = "AWAKENED")]
        Awakened,
        /// <summary>
        /// Indicates that the workflow activity's execution has faulted
        /// </summary>
        [EnumMember(Value = "FAULTED")]
        Faulted,
        /// <summary>
        /// Indicates that the workflow activity has been executed
        /// </summary>
        [EnumMember(Value = "EXECUTED")]
        Executed,
        /// <summary>
        /// Indicates that the workflow activity has been terminated
        /// </summary>
        [EnumMember(Value = "TERMINATED")]
        Terminated,
        /// <summary>
        /// Indicates that the workflow activity has timed out
        /// </summary>
        [EnumMember(Value = "TIMEDOUT")]
        TimedOut
    }

}
