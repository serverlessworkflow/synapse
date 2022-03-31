namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphViewModel
        : IIdentifiable, ILabeled, IDimension, IMetadata
    {
        IDictionary<Guid, INodeViewModel> Nodes { get; set; }
        IDictionary<Guid, IEdgeViewModel> Edges { get; set; }
        IDictionary<Guid, IClusterViewModel> Clusters { get; set; }
        IGraphLib? DagreGraph {  get; set; }
    }
}
