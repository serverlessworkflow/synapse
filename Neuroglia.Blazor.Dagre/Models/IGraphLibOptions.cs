namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphLibOptions
    {
        bool? Compound { get; set; }

        bool? Directed { get; set; }

        bool? Multigraph { get; set; }
    }
}
