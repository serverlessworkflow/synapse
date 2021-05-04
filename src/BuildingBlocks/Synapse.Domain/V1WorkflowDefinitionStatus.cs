using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Synapse.Domain.Models;
using System.Runtime.Serialization;

namespace Synapse
{

    /// <summary>
    /// Enumerates all possible values for <see cref="V1Workflow"/> statuses
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1WorkflowDefinitionStatus
    {
        /// <summary>
        /// Indicates that the <see cref="V1Workflow"/> is pending processing
        /// </summary>
        [EnumMember(Value = "PENDING")]
        Pending,
        /// <summary>
        /// Indicates that the <see cref="V1Workflow"/> is being processed by an operator
        /// </summary>
        [EnumMember(Value = "PROCESSING")]
        Processing,
        /// <summary>
        /// Indicates that the <see cref="V1Workflow"/> has been processed and is valid
        /// </summary>
        [EnumMember(Value = "VALID")]
        Valid,
        /// <summary>
        /// Indicates that the <see cref="V1Workflow"/> has been processed and is invalid
        /// </summary>
        [EnumMember(Value = "INVALID")]
        Invalid,
        /// <summary>
        /// Indicates that an error occured during the <see cref="V1Workflow"/>'s processing
        /// </summary>
        [EnumMember(Value = "ERROR")]
        Error
    }

}
