namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphLibAlg
    {
        /// <summary>
        /// Finds all connected components in a graph and returns an array of these components. Each component is itself an array that contains the ids of nodes in the component.
        /// This function takes O(|V|) time.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        Task<string[][]> ComponentsAsync(IGraphLib graph);
        /// <summary>
        /// This function is an implementation of Dijkstra's algorithm which finds the shortest path from source to all other nodes in g. This function returns a map of v -> { distance, predecessor }. The distance property holds the sum of the weights from source to v along the shortest path or Number.POSITIVE_INFINITY if there is no path from source. The predecessor property can be used to walk the individual elements of the path from source to v in reverse order.
        /// It takes an optional weightFn(e) which returns the weight of the edge e.If no weightFn is supplied then each edge is assumed to have a weight of 1. This function throws an Error if any of the traversed edges have a negative edge weight.
        /// It takes an optional edgeFn(v) which returns the ids of all edges incident to the node v for the purposes of shortest path traversal. By default this function uses the g.outEdges.
        /// It takes O((|E| + |V|) * log |V|) time.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="weightFn"></param>
        /// <param name="edgeFn"></param>
        /// <returns></returns>
        Task<object> DijkstraAsync(IGraphLib graph, string source, Func<IGraphLibEdge, double>? weightFn, Func<string, object>? edgeFn);
        /// <summary>
        /// This function finds the shortest path from each node to every other reachable node in the graph. It is similar to alg.dijkstra, but instead of returning a single-source array, it returns a mapping of of source -> alg.dijksta(g, source, weightFn, edgeFn).
        /// This function takes an optional weightFn(e) which returns the weight of the edge e.If no weightFn is supplied then each edge is assumed to have a weight of 1. This function throws an Error if any of the traversed edges have a negative edge weight.
        /// This function takes an optional edgeFn(u) which returns the ids of all edges incident to the node u for the purposes of shortest path traversal. By default this function uses g.outEdges.
        /// This function takes O(|V| * (|E| + |V|) * log |V|) time.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="weightFn"></param>
        /// <param name="edgeFn"></param>
        /// <returns></returns>
        Task<object> DijkstraAllAsync(IGraphLib graph, string source, Func<IGraphLibEdge, double>? weightFn, Func<string, object>? edgeFn);
        /// <summary>
        /// Given a Graph, g, this function returns all nodes that are part of a cycle.As there may be more than one cycle in a graph this function return an array of these cycles, where each cycle is itself represented by an array of ids for each node involved in that cycle.
        /// alg.isAcyclic is more efficient if you only need to determine whether a graph has a cycle or not.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        Task<object> FindCyclesAsync(IGraphLib graph);
        /// <summary>
        /// This function is an implementation of the Floyd-Warshall algorithm, which finds the shortest path from each node to every other reachable node in the graph. It is similar to alg.dijkstraAll, but it handles negative edge weights and is more efficient for some types of graphs. This function returns a map of source -> { target -> { distance, predecessor }. The distance property holds the sum of the weights from source to target along the shortest path of Number.POSITIVE_INFINITY if there is no path from source. The predecessor property can be used to walk the individual elements of the path from source to target in reverse order.
        /// This function takes an optional weightFn(e) which returns the weight of the edge e.If no weightFunc is supplied then each edge is assumed to have a weight of 1.
        /// This function takes an optional edgeFn(v) which returns the ids of all edges incident to the node v for the purposes of shortest path traversal. By default this function uses the outEdges function on the supplied graph.
        /// This algorithm takes O(|V|^3) time.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="weightFn"></param>
        /// <param name="edgeFn"></param>
        /// <returns></returns>
        Task<object> FloydWarchallAsync(IGraphLib graph, string source, Func<IGraphLibEdge, double>? weightFn, Func<string, object>? edgeFn);
        /// <summary>
        /// Given a Graph, g, this function returns true if the
        /// graph has no cycles and returns false if it does.This algorithm returns as soon as it detects the first cycle.You can use
        /// alg.findCycles to get the actual list of cycles in the graph.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        Task<bool> IsAcyclicAsync(IGraphLib graph);
        /// <summary>
        /// This function performs a postorder traversal of the graph g starting at the nodes vs. For each node visited, v, the function callback(v) is called.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        Task<string[]> PostorderAsync(IGraphLib graph, string nodeName);
        Task<string[]> PostorderAsync(IGraphLib graph, string[] nodeNames);
        /// <summary>
        /// This function performs a preorder traversal of the graph g starting at the nodes vs. For each node visited, v, the function callback(v) is called.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        Task<string[]> PreorderAsync(IGraphLib graph, string nodeName);
        Task<string[]> PreorderAsync(IGraphLib graph, string[] nodeNames);
        /// <summary>
        /// Prim's algorithm takes a connected undirected graph and generates a minimum spanning tree. This function returns the minimum spanning tree as an undirected graph. This algorithm is derived from the description in "Introduction to Algorithms", Third Edition, Cormen, et al., Pg 634.
        /// This function takes a weightFn(e) which returns the weight of the edge e.It throws an Error if the graph is not connected.
        /// This function takes O(|E| log |V|) time.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="weightFn"></param>
        /// <returns></returns>
        Task<IGraphLib> PrimAsync(IGraphLib graph, Func<IGraphLibEdge, double>? weightFn);
        /// <summary>
        /// This function is an implementation of Tarjan's algorithm which finds all strongly connected components in the directed graph g. Each strongly connected component is composed of nodes that can reach all other nodes in the component via directed edges. A strongly connected component can consist of a single node if that node cannot both reach and be reached by any other specific node in the graph. Components of more than one node are guaranteed to have at least one cycle.
        /// This function returns an array of components.Each component is itself an array that contains the ids of all nodes in the component.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        Task<string[][]> TarjamAsync(IGraphLib graph);
        /// <summary>
        /// An implementation of topological sorting.
        /// Given a Graph g this function returns an array of nodes such that for each edge u -> v, u appears before v in the array.If the graph has a cycle it is impossible to generate such a list and CycleException is thrown.
        /// Takes O(|V| + |E|) time.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        Task<string[]> TopsortAsync(IGraphLib graph);
    }
}
