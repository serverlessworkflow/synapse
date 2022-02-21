namespace Synapse.Sdk.Models
{

    /// <summary>
    /// Describes a custom resource definition
    /// </summary>
    public class CustomResourceDefinition
    {

        /// <summary>
        /// Gets the custom resource's api version
        /// </summary>
        [Newtonsoft.Json.JsonProperty("apiVersion")]
        [ProtoBuf.ProtoMember(0, Name = "apiVersion")]
        [System.Text.Json.Serialization.JsonPropertyName("apiVersion")]
        [YamlDotNet.Serialization.YamlMember(Alias = "apiVersion")]
        [Description("The custom resource's api version")]
        public virtual string ApiVersion { get; set; }

        /// <summary>
        /// Gets the custom resource's kind
        /// </summary>
        [Newtonsoft.Json.JsonProperty("kind")]
        [ProtoBuf.ProtoMember(1, Name = "kind")]
        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        [YamlDotNet.Serialization.YamlMember(Alias = "kind")]
        [Description("The custom resource's kind")]
        public virtual string Kind { get; set; }

        /// <summary>
        /// Gets the custom resource's plural form
        /// </summary>
        [Newtonsoft.Json.JsonProperty("plural")]
        [ProtoBuf.ProtoMember(2, Name = "plural")]
        [System.Text.Json.Serialization.JsonPropertyName("plural")]
        [YamlDotNet.Serialization.YamlMember(Alias = "plural")]
        [Description("The custom resource's plural form")]
        public string Plural { get; set; }

        /// <summary>
        /// Gets the custom resource's group
        /// </summary>
        [Newtonsoft.Json.JsonProperty("group")]
        [ProtoBuf.ProtoMember(3, Name = "group")]
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        [YamlDotNet.Serialization.YamlMember(Alias = "group")]
        [Description("The custom resource's group")]
        public string Group { get; set; }

        /// <summary>
        /// Gets the custom resource's version
        /// </summary>
        [Newtonsoft.Json.JsonProperty("version")]
        [ProtoBuf.ProtoMember(3, Name = "version")]
        [System.Text.Json.Serialization.JsonPropertyName("version")]
        [YamlDotNet.Serialization.YamlMember(Alias = "version")]
        [Description("The custom resource's version")]
        public string Version { get; set; }

    }

}
