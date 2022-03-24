namespace Neuroglia.BlazorDagre.Models
{
    public class GraphViewModel
        : BaseViewModel, IGraphViewModel
    {
        public virtual ICollection<INodeViewModel> Nodes { get; set; }
        public virtual ICollection<IEdgeViewModel> Edges { get; set; }
        public virtual ICollection<IClusterViewModel> Clusters { get; set; }
        public virtual double Width { get; set; }
        public virtual double Height { get; set; }

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
