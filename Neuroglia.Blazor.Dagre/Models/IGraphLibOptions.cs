namespace Neuroglia.Blazor.Dagre.Models
{
    /// <summary>
    /// Represents the options used to construct a new <see cref="IGraphLib"/>
    /// </summary>
    public interface IGraphLibOptions
    {
        /// <summary>
        /// Set to true to allow a graph to have compound nodes - nodes which can be the parent of other nodes. Default: false.
        /// </summary>
        /// 
        bool? Compound { get; set; }

        /// <summary>
        /// Set to true to get a directed graph and false to get an undirected graph. An undirected graph does not treat the order of nodes in an edge as significant. In other words, g.edge("a", "b") === g.edge("b", "a") for an undirected graph. Default: true.
        /// </summary>
        bool? Directed { get; set; }

        /// <summary>
        /// Set to true to allow a graph to have multiple edges between the same pair of nodes. Default: false.
        /// </summary>
        bool? Multigraph { get; set; }
    }
}
