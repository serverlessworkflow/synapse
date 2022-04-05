namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IEdgeViewModel
        : IGraphElement
    {
        Guid SourceId { get; set; }
        Guid TargetId { get; set; }
        ICollection<IPosition> Points { get; set; }
        string? LabelPosition { get; set; }
        double? LabelOffset { get; set; }
        string Shape { get; set; }
        string? StartMarkerId { get; set; }
        string? EndMarkerId { get; set; }
        IBoundingBox BBox { get; }
    }
}
