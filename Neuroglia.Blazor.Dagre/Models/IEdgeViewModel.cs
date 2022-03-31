namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IEdgeViewModel
        : IIdentifiable, ILabeled, IMetadata
    {
        Guid SourceId { get; set; }
        Guid TargetId { get; set; }
        ICollection<IPosition> Points { get; set; }
    }
}
