namespace Neuroglia.Blazor.Dagre.Models
{
    public interface INodeViewModel
        : IGraphElement, IPosition, IDimension, IRadius, IPadding
    {
        Guid? ParentId { get; set; }
        string? Shape { get; set; }
        IBoundingBox? BBox { get; }
    }
}
