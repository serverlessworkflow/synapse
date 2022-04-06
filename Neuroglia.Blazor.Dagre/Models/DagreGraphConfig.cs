namespace Neuroglia.Blazor.Dagre.Models
{
    /// <inheritdoc />
    public class DagreGraphConfig 
        : IDagreGraphConfig
    {
        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("rankdir")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "rankdir", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Direction { get; set; } = null;

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("align")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "align", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Alignment { get; set; } = null;

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("nodesep")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "nodesep", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? NodeSeparation { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("edgesep")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "edgesep", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? EdgeSeparation { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("ranksep")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "ranksep", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? RankSeparation { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("marginx")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "marginx", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? MarginX { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("marginy")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "rankdir", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? MarginY { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("acyclicer")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "acyclicer", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Acyclicer { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("ranker")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(PropertyName = "ranker", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Ranker { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Width { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Height { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual string? Label { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [System.Text.Json.Serialization.JsonExtensionData]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonExtensionData]
        public virtual IDictionary<string, object>? Metadata { get; set; }

        public DagreGraphConfig() { }

        public DagreGraphConfig(IDagreGraphConfig? config = null)
            : this(
                  config?.Direction,
                  config?.Alignment,
                  config?.NodeSeparation,
                  config?.EdgeSeparation,
                  config?.RankSeparation,
                  config?.MarginX,
                  config?.MarginY,
                  config?.Acyclicer,
                  config?.Ranker,
                  config?.Width,
                  config?.Height,
                  config?.Label,
                  config?.Metadata
            )
        {}

        public DagreGraphConfig(
            string? direction = null,
            string? alignment = null,
            double? nodeSeparation = null,
            double? edgeSeparation = null,
            double? rankSeparation = null,
            double? marginX = null,
            double? marginY = null,
            string? acyclicer = null,
            string? ranker = null,
            double? width = null,
            double? height = null,
            string? label = null,
            IDictionary<string, object>? metadata = null
        )
        {
            this.Direction = direction;
            this.Alignment = alignment;
            this.NodeSeparation = nodeSeparation;
            this.EdgeSeparation = edgeSeparation;
            this.RankSeparation = rankSeparation;
            this.MarginX = marginX;
            this.MarginY = marginY;
            this.Acyclicer = acyclicer;
            this.Ranker = ranker;
            this.Width = width;
            this.Height = height;
            this.Label = label;
            this.Metadata = metadata;
        }
    }
}
