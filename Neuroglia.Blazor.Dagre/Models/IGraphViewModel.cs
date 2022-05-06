using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Neuroglia.Blazor.Dagre.Models
{
    public delegate Task MouseEventHandler(ElementReference sender, MouseEventArgs e, IGraphElement? element);
    public delegate Task WheelEventHandler(ElementReference sender, WheelEventArgs e, IGraphElement? element);

    public interface IGraphViewModel
        : IIdentifiable, ILabeled, IDimension, IPosition, ICssClass, IMetadata
    {
        decimal Scale { get; set; }
        bool ShowConstruction { get; set; }
        IReadOnlyDictionary<Guid, INodeViewModel> Nodes { get; }
        IReadOnlyDictionary<Guid, INodeViewModel> AllNodes { get; }
        IReadOnlyDictionary<Guid, IEdgeViewModel> Edges { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> Clusters { get; }
        IReadOnlyDictionary<Guid, IClusterViewModel> AllClusters { get; }
        IReadOnlyCollection<Type> SvgDefinitionComponents { get; }
        IGraphLib? DagreGraph {  get; set; }

        event MouseEventHandler? MouseMove;
        event MouseEventHandler? MouseDown;
        event MouseEventHandler? MouseUp;
        event MouseEventHandler? MouseEnter;
        event MouseEventHandler? MouseLeave;
        event WheelEventHandler? Wheel;

        Task RegisterComponentTypeAsync<TElement, TComponent>()
            where TElement : IGraphElement
            where TComponent : ComponentBase;

        Type GetComponentTypeAsync<TElement>(TElement node)
            where TElement : IGraphElement;

        Task AddElementAsync(IGraphElement element);

        Task AddElementsAsync(IEnumerable<IGraphElement> elements);

        Task OnMouseMoveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element);

        Task OnMouseDownAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element);

        Task OnMouseUpAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element);

        Task OnMouseEnterAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element);

        Task OnMouseLeaveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element);

        Task OnWheelAsync(ElementReference sender, WheelEventArgs e, IGraphElement? element);

    }
}
