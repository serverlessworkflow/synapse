using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphViewModel
        : IIdentifiable, ILabeled, IDimension, IPosition, ICssClass, IMetadata
    {
        decimal Scale { get; set; }
        IReadOnlyDictionary<Guid, INodeViewModel> Nodes { get; }
        IReadOnlyDictionary<Guid, INodeViewModel> AllNodes { get; }
        IReadOnlyDictionary<Guid, IEdgeViewModel> Edges { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> Clusters { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> AllClusters { get; }
        IReadOnlyCollection<Type> SvgDefinitionComponents { get; }
        IGraphLib? DagreGraph {  get; set; }

        event Action<IGraphElement?, MouseEventArgs>? MouseMove;
        event Action<IGraphElement?, MouseEventArgs>? MouseDown;
        event Action<IGraphElement?, MouseEventArgs>? MouseUp;
        event Action<IGraphElement?, WheelEventArgs>? Wheel;

        Task Center();

        Task ZoomToFit();

        Task RegisterComponentTypeAsync<TElement, TComponent>()
            where TElement : IGraphElement
            where TComponent : ComponentBase;

        Type GetComponentTypeAsync<TElement>(TElement node)
            where TElement : IGraphElement;

        Task AddElementAsync(IGraphElement element);

        void OnMouseMove(IGraphElement? element, MouseEventArgs e);

        void OnMouseDown(IGraphElement? element, MouseEventArgs e);

        void OnMouseUp(IGraphElement? element, MouseEventArgs e);

        void OnWheel(IGraphElement? element, WheelEventArgs e);

    }
}
