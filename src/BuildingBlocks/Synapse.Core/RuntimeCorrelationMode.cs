using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Synapse
{
    /// <summary>
    /// Enumerates all supported runtime correlation modes
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuntimeCorrelationMode
    {
        /// <summary>
        /// Indicates that the runtime actibvely for inbound <see cref="CloudEvent"/>s for a specified amount of time before shutting down and waiting for a Synapse Correlator to restart it upon consumption of correlated <see cref="CloudEvent"/>s
        /// </summary>
        [EnumMember(Value = "DUAL")]
        Dual,
        /// <summary>
        /// Indicates that the runtime actively listens for inbound <see cref="CloudEvent"/>s
        /// </summary>
        [EnumMember(Value = "ACTIVE")]
        Active,
        /// <summary>
        /// Indicates that when requested to consume <see cref="CloudEvent"/>s, the runtime shuts down and waits for a Synapse Correlator to restart it upon consumption of correlated <see cref="CloudEvent"/>s
        /// </summary>
        [EnumMember(Value = "PASSIVE")]
        Passive
    }

}
