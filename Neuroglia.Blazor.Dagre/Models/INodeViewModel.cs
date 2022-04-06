namespace Neuroglia.Blazor.Dagre.Models
{
    public interface INodeViewModel
        : IGraphElement, IPosition, IDimension, IRadius
    {
        Guid? ParentId { get; set; }
        string? Shape { get; set; }
        IBoundingBox? BBox { get; }
        void SetGeometry(double? x, double? y, double? width, double? height);
        void Move(double deltaX, double deltaY);
        event Action? Changed;
    }
}
