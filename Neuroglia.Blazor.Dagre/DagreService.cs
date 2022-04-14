using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    /// <summary>
    /// Wraps Dagre js library as an injectable service
    /// https://github.com/dagrejs/dagre
    /// </summary>
    public class DagreService
        : IDagreService
    {
        /// <summary>
        /// The JS Runtime instance
        /// </summary>
        protected readonly IJSRuntime jsRuntime;

        /// <summary>
        /// Creates a new instance of DagreService
        /// </summary>
        /// <param name="jSRuntime"></param>
        public DagreService(IJSRuntime jSRuntime) { 
            this.jsRuntime = jSRuntime; 
        }

        /// <summary>
        /// Computes the nodes and edges position of the provided <see cref="IGraphViewModel"/>
        /// </summary>
        /// <param name="graphViewModel"></param>
        /// <param name="options"></param>
        /// <returns>The updated <see cref="IGraphViewModel"/></returns>
        public virtual async Task<IGraphViewModel> ComputePositionsAsync(IGraphViewModel graphViewModel, IDagreGraphOptions? options = null)
        {
            // build the dagre/graphlib graph
            var graph = await this.GraphAsync(options);
            var nodes = graphViewModel.AllNodes.Values.Concat(
                graphViewModel.AllClusters.Values
            );
            foreach (var node in nodes)
            {
                await graph.SetNodeAsync(node.Id.ToString(), node);
                if (node.ParentId != null) {
                    await graph.SetParentAsync(node.Id.ToString(), node.ParentId.ToString()!);
                }
            }
            foreach(var edge in graphViewModel.Edges.Values)
            {
                if (options?.Multigraph == true)
                {
                    await graph.SetEdgeAsync(edge.SourceId.ToString(), edge.TargetId.ToString(), edge, edge.Id.ToString());
                }
                else
                {
                    await graph.SetEdgeAsync(edge.SourceId.ToString(), edge.TargetId.ToString(), edge);
                }
            }
            await this.LayoutAsync(graph);
            // update our viewmodels with the computed values
            foreach (var node in nodes)
            {
                var graphNode = await graph.NodeAsync(node.Id.ToString());
                node.SetGeometry(graphNode.X, graphNode.Y, graphNode.Width, graphNode.Height);
            }
            foreach (var edge in graphViewModel.Edges.Values)
            {
                GraphLibEdge graphEdge;
                if (options?.Multigraph == true)
                {
                    graphEdge = await graph.EdgeAsync(edge.SourceId.ToString(), edge.TargetId.ToString(), edge.Id.ToString());
                }
                else {

                    graphEdge = await graph.EdgeAsync(edge.SourceId.ToString(), edge.TargetId.ToString());
                }
                if (graphEdge?.Points != null)
                {
                    edge.Points = graphEdge.Points.ToList<IPosition>();
                }
            }
            graphViewModel.DagreGraph = graph;
            return graphViewModel;
        }

        /// <inheritdoc/>
        public virtual async Task<IGraphLib> DeserializeAsync(string json) => await this.jsRuntime.InvokeAsync<IGraphLib>("neuroglia.blazor.dagre.read", json);

        /// <summary>
        /// Returns a new <see cref="IGraphLib"/> instance
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> GraphAsync(IDagreGraphOptions? options = null)
        {
            var graphLibOptions = new GraphLibOptions(options);
            if (graphLibOptions.Multigraph is null) graphLibOptions.Multigraph = true;
            if (graphLibOptions.Compound is null) graphLibOptions.Compound = true;
            var jsIntance = await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.graph", graphLibOptions);
            var graph = new GraphLib(jsIntance);
            var dagreGraphConfig = new DagreGraphConfig(options);
            if (dagreGraphConfig.Direction is null) dagreGraphConfig.Direction = DagreGraphDirection.LeftToRight;
            await graph.SetGraphAsync(dagreGraphConfig);
            return graph;
        }

        /// <summary>
        /// Computes the graph layout
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib?> LayoutAsync(IGraphLib graph) => await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.layout", await graph.InstanceAsync()) as IGraphLib;
        
        /// <inheritdoc/>
        public virtual async Task<string> SerializeAsync(IGraphLib graph) => await this.jsRuntime.InvokeAsync<string>("neuroglia.blazor.dagre.write", await graph.InstanceAsync());
    }
}
