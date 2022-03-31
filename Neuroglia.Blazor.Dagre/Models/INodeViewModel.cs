namespace Neuroglia.Blazor.Dagre.Models
{
    public interface INodeViewModel
        : IIdentifiable, ILabeled, IPosition, IDimension, IMetadata, IRadius, IPadding
    {
        Guid? ParentId { get; set; }
    }
}
