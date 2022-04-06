namespace Neuroglia.Blazor.Dagre.Models
{
    /// <summary>
    /// Represents a <see cref="IGraphLib"/> edge
    /// </summary>
    public interface IGraphLibEdge
        : IMetadata
    {
        /// <summary>
        /// The name that uniquely identifies a multi-edge.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// The id of one node
        /// </summary>
        string V { get; set; }

        /// <summary>
        /// The id of the other node
        /// </summary>
        string W { get; set; }

        /// <summary>
        /// The edge points
        /// </summary>
        IPosition[]? Points { get; set; }

    }
}
