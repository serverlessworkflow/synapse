namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphLibNode
        : IIdentifiable, IDimension, IPosition, ILabeled, IMetadata
    {
        string? Class { get; set; }

        double? Rx { get; set; }

        double? Ry { get; set; }

        string? Shape { get; set; }
    }
}
