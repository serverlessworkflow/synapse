namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphViewModel
        : IIdentifiable, ILabeled, IDimension, IMetadata
    {
        ICollection<INodeViewModel> Nodes { get; set; }
        ICollection<IEdgeViewModel> Edges { get; set; }
        ICollection<IClusterViewModel> Clusters { get; set; }
    }
}
