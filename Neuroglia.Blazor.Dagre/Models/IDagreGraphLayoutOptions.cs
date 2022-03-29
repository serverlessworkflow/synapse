namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IDagreGraphLayoutOptions
    {
        /// <summary>
        /// Direction for rank nodes. Can be TB, BT, LR, or RL, where T = top, B = bottom, L = left, and R = right.
        /// The static class <see cref="DagreGraphDirection"/> can be used: DagreGraphDirection.TopToBottom, DagreGraphDirection.BottomToTop, DagreGraphDirection.LeftToRight, DagreGraphDirection.RightToLeft
        /// Default: TB
        /// </summary>
        string? Direction {  get; set; }

        /// <summary>
        /// Alignment for rank nodes. Can be UL, UR, DL, or DR, where U = up, D = down, L = left, and R = right.
        /// The static class <see cref="DagreGraphAlignment"/> can be used: DagreGraphAlignment.UpLeft, DagreGraphAlignment.UpRight, DagreGraphAlignment.DownLeft, DagreGraphAlignment.DownRight
        /// Default: undefined
        /// </summary>
        string? Alignment { get; set; }

        /// <summary>
        /// Number of pixels that separate nodes horizontally in the layout.
        /// Default: 50
        /// </summary>
        double? NodeSeparation { get; set; }

        /// <summary>
        /// Number of pixels that separate edges horizontally in the layout.
        /// Default: 10
        /// </summary>
        double? EdgeSeparation { get; set; }

        /// <summary>
        /// Number of pixels between each rank in the layout.
        /// Default: 50
        /// </summary>
        double? RankSeparation { get; set; }

        /// <summary>
        /// Number of pixels to use as a margin around the left and right of the graph.
        /// Default: 0
        /// </summary>
        double? MarginX { get; set; }

        /// <summary>
        /// Number of pixels to use as a margin around the top and bottom of the graph.
        /// Default: 0
        /// </summary>
        double? MarginY { get; set; }

        /// <summary>
        /// If set to greedy, uses a greedy heuristic for finding a feedback arc set for a graph. A feedback arc set is a set of edges that can be removed to make a graph acyclic.
        /// Default: undefined
        /// </summary>
        string? Acyclicer { get; set; }

        /// <summary>
        /// Type of algorithm to assigns a rank to each node in the input graph. Possible values: network-simplex, tight-tree or longest-path

        /// The static class <see cref="DagreGraphRanker"/> can be used: DagreGraphRanker.NetworkSimplex, DagreGraphRanker.TightTree, DagreGraphRanker.LongestPath
        /// Default: network-simplex
        /// </summary>
        string? Ranker { get; set; }
    }

}
