namespace Neuroglia.Blazor.Dagre.Models
{
    public interface IGraphLibAlg
    {
        Task<string[][]> Components(IGraphLib graph);
        Task<object> Dijkstra(IGraphLib graph, string source, Func<IGraphLibEdge, double>? weightFn, Func<string, object>? edgeFn);
        Task<object> DijkstraAll(IGraphLib graph, string source, Func<IGraphLibEdge, double>? weightFn, Func<string, object>? edgeFn);
        Task<object> FindCycles(IGraphLib graph);
        Task<object> FloydWarchall(IGraphLib graph, string source, Func<IGraphLibEdge, double>? weightFn, Func<string, object>? edgeFn);
        Task<bool> IsAcyclic(IGraphLib graph);
        Task<string[]> Postorder(IGraphLib graph, string nodeName);
        Task<string[]> Postorder(IGraphLib graph, string[] nodeNames);
        Task<string[]> Preorder(IGraphLib graph, string nodeName);
        Task<string[]> Preorder(IGraphLib graph, string[] nodeNames);
        Task<IGraphLib> Prim(IGraphLib graph, Func<IGraphLibEdge, double>? weightFn);
        Task<string[][]> Tarjam(IGraphLib graph);
        Task<string[]> Topsort(IGraphLib graph);
    }
}
