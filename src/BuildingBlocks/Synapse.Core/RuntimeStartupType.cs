using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Synapse
{
    /// <summary>
    /// Enumerates all supported types of runtime startups
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuntimeStartupType
    {
        /// <summary>
        /// Indicates that the runtime has been explicitely started, by creating a new workflow instance
        /// </summary>
        [EnumMember(Value = "EXPLICIT")]
        Explicit,
        /// <summary>
        /// Indicates that the runtime has been started by a schedule (CronJob or Job)
        /// </summary>
        [EnumMember(Value = "SCHEDULE")]
        Schedule,
        /// <summary>
        /// Indicates that the runtime has been started by a trigger
        /// </summary>
        [EnumMember(Value = "TRIGGER")]
        Trigger
    }

}
