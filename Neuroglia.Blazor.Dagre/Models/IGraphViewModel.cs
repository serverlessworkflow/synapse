using Microsoft.AspNetCore.Components;

namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphViewModel
        : IIdentifiable, ILabeled, IDimension, IMetadata
    {
        IReadOnlyDictionary<Guid, INodeViewModel> Nodes { get; }
        IReadOnlyDictionary<Guid, INodeViewModel> AllNodes { get; }
        IReadOnlyDictionary<Guid, IEdgeViewModel> Edges { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> Clusters { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> AllClusters { get; }
        IReadOnlyCollection<Type> SvgDefinitionComponents { get; }
        IGraphLib? DagreGraph {  get; set; }

        Task RegisterComponentType<TElement, TComponent>()
            where TElement : IGraphElement
            where TComponent : ComponentBase;

        Type GetComponentType<TElement>(TElement node)
            where TElement : IGraphElement;
    }
}
