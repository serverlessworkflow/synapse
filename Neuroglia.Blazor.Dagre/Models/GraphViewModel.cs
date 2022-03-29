namespace Neuroglia.Blazor.Dagre.Models
{
    public class GraphViewModel
        : GraphElement, IGraphViewModel
    {
        public virtual ICollection<INodeViewModel> Nodes { get; set; }
        public virtual ICollection<IEdgeViewModel> Edges { get; set; }
        public virtual ICollection<IClusterViewModel> Clusters { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Width { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Height { get; set; }

        public GraphViewModel(
            ICollection<INodeViewModel> nodes,
            ICollection<IEdgeViewModel> edges,
            ICollection<IClusterViewModel> clusters,
            double width,
            double height
        )
            : base()
        {
            this.Nodes = nodes;
            this.Edges = edges;
            this.Clusters = clusters;
            this.Width = width;
            this.Height = height;
        }

    }
}
