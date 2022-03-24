namespace Neuroglia.BlazorDagre.Models
{
    public interface IEdgeViewModel
        : IIdentifiable, ILabeled, IMetadata
    {
        Guid SourceId { get; set; }
        Guid TargetId { get; set; }
    }
}
