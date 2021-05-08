using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Synapse.Domain.Models;
using System.Runtime.Serialization;

namespace Synapse
{
    /// <summary>
    /// Enumerates all supported <see cref="V1Trigger"/> correlation modes
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1TriggerCorrelationMode
    {
        /// <summary>
        /// Indicates that the trigger is pending activation
        /// </summary>
        [EnumMember(Value = "EXCLUSIVE")]
        Exclusive,
        /// <summary>
        /// Indicates that the trigger is pending activation
        /// </summary>
        [EnumMember(Value = "PARALLEL")]
        Parallel
    }

}
