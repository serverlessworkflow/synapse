using System.Collections.ObjectModel;

namespace Neuroglia.Blazor.Dagre.Models
{
    public class GraphViewModel
        : GraphElement, IGraphViewModel
    {
        public virtual IDictionary<Guid, INodeViewModel> Nodes { get; set; }
        public virtual IDictionary<Guid, IEdgeViewModel> Edges { get; set; }
        public virtual IDictionary<Guid, IClusterViewModel> Clusters { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual IGraphLib? DagreGraph { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Width { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Height { get; set; }

        public GraphViewModel(
            IDictionary<Guid, INodeViewModel>? nodes = null,
            IDictionary<Guid, IEdgeViewModel>? edges = null,
            IDictionary<Guid, IClusterViewModel>? clusters = null,
            double? width = null,
            double? height = null
        )
            : base()
        {
            this.Nodes = nodes ?? new Dictionary<Guid, INodeViewModel>();
            this.Edges = edges ?? new Dictionary<Guid, IEdgeViewModel>(); ;
            this.Clusters = clusters ?? new Dictionary<Guid, IClusterViewModel>();
            this.Width = width;
            this.Height = height;
        }

    }
}
