namespace Neuroglia.Blazor.Dagre.Models
{

    /// <inheritdoc />
    public class GraphLibEdge
        : IGraphLibEdge
    {
        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [System.Text.Json.Serialization.JsonExtensionData]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonExtensionData]
        public virtual IDictionary<string, object>? Metadata { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        /// <inheritdoc />
        public virtual string? Name { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        /// <inheritdoc />
        public virtual string V { get; set; } = "";

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        /// <inheritdoc />
        public virtual string W { get; set; } = "";

    }
}
