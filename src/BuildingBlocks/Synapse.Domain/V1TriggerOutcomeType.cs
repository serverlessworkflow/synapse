using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Synapse
{

    /// <summary>
    /// Enumerates all possible types of trigger outcomes 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1TriggerOutcomeType
    {
        /// <summary>
        /// Indicates that the trigger should create and run a new workflow instance
        /// </summary>
        [EnumMember(Value = "RUN")]
        Run,
        /// <summary>
        /// Indicates that the trigger should resume the execution of an existing workflow instance
        /// </summary>
        [EnumMember(Value = "RESUME")]
        Resume
    }

}
