
namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphElement
        : IIdentifiable, ILabeled, IMetadata
    {
        Type? ComponentType { get; set; }
    }
}
