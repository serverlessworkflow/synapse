using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Synapse.Domain.Models;
using System.Runtime.Serialization;

namespace Synapse
{
    /// <summary>
    /// Enumerates all supported <see cref="V1Trigger"/> condition types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1TriggerConditionType
    {
        /// <summary>
        /// Indicates that the trigger should fire if any of its conditions are met
        /// </summary>
        [EnumMember(Value = "ANY")]
        AnyOf,
        /// <summary>
        /// Indicates that the trigger should fire only if all of its conditions are met
        /// </summary>
        [EnumMember(Value = "ALL")]
        AllOf
    }

}
