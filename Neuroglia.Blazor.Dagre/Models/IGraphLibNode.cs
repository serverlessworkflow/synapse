namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphLibNode
        : IIdentifiable, IDimension, IPosition, IPadding, ILabeled, IMetadata
    {
        string? Class { get; set; }

        double? Padding { get; set; }

        double? Rx { get; set; }

        double? Ry { get; set; }

        string? Shape { get; set; }
    }
}
