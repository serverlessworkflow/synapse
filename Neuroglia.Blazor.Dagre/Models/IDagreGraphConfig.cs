namespace Neuroglia.Blazor.Dagre.Models
{
    /// <summary>
    /// Represents the <see cref="IGraphLib.SetGraph(object)"/> configuration used by Dagre
    /// </summary>
    public interface IDagreGraphConfig
        : IDagreGraphLayoutOptions, ILabeled, IDimension, IMetadata
    {
    }
}
