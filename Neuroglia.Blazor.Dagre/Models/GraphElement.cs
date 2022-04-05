namespace Neuroglia.Blazor.Dagre.Models
{
    /// <summary>
    /// Represents a basic element of <see cref="IGraphViewModel"/>
    /// </summary>
    public abstract class GraphElement
        : IGraphElement
    {
        /// <inheritdoc />
        public virtual Guid Id { get; set; } = Guid.NewGuid();

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Label { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual Type? ComponentType { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? CssClass { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [System.Text.Json.Serialization.JsonExtensionData]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonExtensionData]
        public virtual IDictionary<string, object>? Metadata { get; set; }

        protected GraphElement() 
        { }

        protected GraphElement(string? label = "", string? cssClass = null, Type? componentType = null) {
            this.Label = label;
            this.CssClass = cssClass;
            this.ComponentType = componentType;
        }

    }
}
