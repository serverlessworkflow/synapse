namespace Neuroglia.Blazor.Dagre.Models
{
    /// <inheritdoc />
    public class GraphLibOptions
        : IGraphLibOptions
    {
        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual bool? Compound { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual bool? Directed { get; set; }

        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual bool? Multigraph { get; set; }

        public GraphLibOptions() { }

        public GraphLibOptions(IGraphLibOptions? options = null)
            : this(options?.Compound, options?.Directed, options?.Multigraph)
        {}

        public GraphLibOptions(bool? compound = null, bool? directed = null, bool? multigraph = null)
        {
            this.Compound = compound;
            this.Directed = directed;
            this.Multigraph = multigraph;
        }
    }
}
