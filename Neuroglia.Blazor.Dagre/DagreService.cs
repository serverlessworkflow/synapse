using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;
using System.Dynamic;

namespace Neuroglia.Blazor.Dagre
{
    /// <summary>
    /// Wraps Dagre js library as an injectable service
    /// https://github.com/dagrejs/dagre
    /// </summary>
    public class DagreService
        : IGraphLibJsonConverter
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
        public virtual async Task<IGraphViewModel> ComputePositions(IGraphViewModel graphViewModel, IDagreGraphOptions? options = null)
        {
            // build the dagre/graphlib graph
            var graph = await this.Graph(options);
            var nodes = graphViewModel.Nodes.Values.Concat(
                graphViewModel.Clusters.Values.SelectMany(cluster => this.FlattenNodes(cluster))
            );
            foreach (var node in nodes)
            {
                await graph.SetNode(node.Id.ToString(), node);
                if (node.ParentId != null) {
                    await graph.SetParent(node.Id.ToString(), node.ParentId.ToString()!);
                }
            }
            foreach(var edge in graphViewModel.Edges.Values)
            {
                if (options?.Multigraph == true)
                {
                    await graph.SetEdge(edge.SourceId.ToString(), edge.TargetId.ToString(), edge, edge.Id.ToString());
                }
                else
                {
                    await graph.SetEdge(edge.SourceId.ToString(), edge.TargetId.ToString(), edge);
                }
            }
            await this.Layout(graph);
            // update our viewmodels with the computed values
            foreach (var node in nodes)
            {
                var graphNode = await graph.Node(node.Id.ToString());
                node.Width = graphNode.Width;
                node.Height = graphNode.Height;
                node.X = graphNode.X;
                node.Y = graphNode.Y;
            }
            foreach (var edge in graphViewModel.Edges.Values)
            {
                GraphLibEdge graphEdge;
                if (options?.Multigraph == true)
                {
                    graphEdge = await graph.Edge(edge.SourceId.ToString(), edge.TargetId.ToString(), edge.Id.ToString());
                }
                else {

                    graphEdge = await graph.Edge(edge.SourceId.ToString(), edge.TargetId.ToString());
                }
                if (graphEdge?.Points != null)
                {
                    edge.Points = graphEdge.Points.ToList<IPosition>();
                }
            }
            graphViewModel.DagreGraph = graph;
            return graphViewModel;
        }

        /// <summary>
        /// Recursively flatten the <see cref="INodeViewModel"/> of the provided <see cref="IClusterViewModel"/>
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        protected virtual IEnumerable<INodeViewModel> FlattenNodes(IClusterViewModel cluster)
        {
            var flattened = new List<INodeViewModel>();
            if (cluster == null)
            {
                return flattened;
            }
            flattened.Add(cluster);
            foreach(var child in cluster.Children.Values)
            {
                if (child == null)
                {
                    continue;
                }
                child.ParentId = cluster.Id;
                if (child is IClusterViewModel clusterViewModel)
                {
                    flattened.AddRange(this.FlattenNodes(clusterViewModel));
                }
                else if (child is INodeViewModel nodeViewModel)
                {
                    flattened.Add(nodeViewModel);
                }
            }
            return flattened;
        }

        /// <summary>
        /// Recursively flatten the <see cref="INodeViewModel"/> of the provided <see cref="IClusterViewModel"/>
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<INodeViewModel>> FlattenNodesAsync(IClusterViewModel cluster)
        {
            var flattened = new List<INodeViewModel>();
            if (cluster == null)
            {
                return flattened;
            }
            flattened.Add(cluster);
            foreach (var child in cluster.Children.Values)
            {
                if (child == null)
                {
                    continue;
                }
                child.ParentId = cluster.Id;
                if (child is IClusterViewModel clusterViewModel)
                {
                    flattened.AddRange(await this.FlattenNodesAsync(clusterViewModel));
                }
                else if (child is INodeViewModel nodeViewModel)
                {
                    flattened.Add(nodeViewModel);
                }
            }
            return flattened;
        }

        /// <inheritdoc/>
        public virtual async Task<IGraphLib> Deserialize(string json) => await this.jsRuntime.InvokeAsync<IGraphLib>("neuroglia.blazor.dagre.read", json);

        /// <summary>
        /// Returns a new <see cref="IGraphLib"/> instance
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> Graph(IDagreGraphOptions? options = null)
        {
            var graphLibOptions = new GraphLibOptions(options);
            if (graphLibOptions.Multigraph is null) graphLibOptions.Multigraph = true;
            if (graphLibOptions.Compound is null) graphLibOptions.Compound = true;
            var jsIntance = await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.graph", graphLibOptions);
            var graph = new GraphLib(jsIntance);
            var dagreGraphConfig = new DagreGraphConfig(options);
            if (dagreGraphConfig.Direction is null) dagreGraphConfig.Direction = DagreGraphDirection.LeftToRight;
            await graph.SetGraph(dagreGraphConfig);
            return graph;
        }

        /// <summary>
        /// Computes the graph layout
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public virtual async Task<IGraphLib> Layout(IGraphLib graph) => await this.jsRuntime.InvokeAsync<IJSObjectReference>("neuroglia.blazor.dagre.layout", await graph.Instance()) as IGraphLib;
        
        /// <inheritdoc/>
        public virtual async Task<string> Serialize(IGraphLib graph) => await this.jsRuntime.InvokeAsync<string>("neuroglia.blazor.dagre.write", await graph.Instance());
    }
}
