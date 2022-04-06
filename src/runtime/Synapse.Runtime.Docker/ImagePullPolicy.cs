using System.Runtime.Serialization;

namespace Synapse.Runtime.Docker
{

    /// <summary>
    /// Enumerates all supported Docker image pull policies
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum ImagePullPolicy
    {
        /// <summary>
        /// Indicates that the image should only be pulled when not present
        /// </summary>
        [EnumMember(Value = "IfNotPresent")]
        IfNotPresent,
        /// <summary>
        /// Indicates that the image should always be pulled
        /// </summary>
        [EnumMember(Value = "Always")]
        Always
    }

}
