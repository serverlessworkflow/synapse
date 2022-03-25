namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphLibNode
    {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        string? Class { get; set; }
        string? Label { get; set; }
        double? Padding { get; set; }
        double? PaddingX { get; set; }
        double? PaddingY { get; set; }
        double? Rx { get; set; }
        double? Ry { get; set; }
        string? Shape { get; set; }

    }
}
