using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    /// <inheritdoc/>
    public class GraphLib
        : IGraphLib
    {
        /// <summary>
        /// The Graph js instance
        /// </summary>
        protected readonly IJSObjectReference jsInstance;

        public GraphLib(IJSObjectReference jsInstance)
        {
            this.jsInstance = jsInstance;
        }
        /// <inheritdoc/>
        public virtual async Task<string[]> Children(string v) => await this.jsInstance.InvokeAsync<string[]>("children", v);
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> FilterNodes(Func<string, bool> filter) => await this.jsInstance.InvokeAsync<IJSObjectReference>("filterNodes", filter) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IDagreGraphConfig?> Graph() => await this.jsInstance.InvokeAsync<DagreGraphOptions?>("graph");
        /// <inheritdoc/>
        public virtual async Task<bool> HasEdge(string v, string w, string name) => await this.jsInstance.InvokeAsync<bool>("hasEdge", v, w, name);
        /// <inheritdoc/>
        public virtual async Task<bool> HasEdge(GraphLibEdge edge) => await this.jsInstance.InvokeAsync<bool>("hasEdge", edge);
        /// <inheritdoc/>
        public virtual async Task<bool> HasNode(string name) => await this.jsInstance.InvokeAsync<bool>("hasNode", name);
        /// <inheritdoc/>
        public virtual async Task<object> Edge(string v, string w, string name) => await this.jsInstance.InvokeAsync<object>("edge", v, w, name);
        /// <inheritdoc/>
        public virtual async Task<object> Edge(GraphLibEdge e) => await this.jsInstance.InvokeAsync<object>("edge", e);
        /// <inheritdoc/>
        public virtual async Task<double> EdgeCount() => await this.jsInstance.InvokeAsync<double>("edgeCount");
        /// <inheritdoc/>
        public virtual async Task<GraphLibEdge[]> Edges() => await this.jsInstance.InvokeAsync<GraphLibEdge[]>("edges");
        /// <inheritdoc/>
        public virtual async Task<GraphLibEdge[]?> InEdges(string v, string w) => await this.jsInstance.InvokeAsync<GraphLibEdge[]?>("inEdges", v, w);
        /// <inheritdoc/>
        public virtual async Task<bool> IsCompound() => await this.jsInstance.InvokeAsync<bool>("isCompound");
        /// <inheritdoc/>
        public virtual async Task<bool> IsDirected() => await this.jsInstance.InvokeAsync<bool>("isDirected");
        /// <inheritdoc/>
        public virtual async Task<bool> IsMultigraph() => await this.jsInstance.InvokeAsync<bool>("isMultigraph");
        /// <inheritdoc/>
        public virtual async Task<IJSObjectReference> Instance() => await Task.FromResult(this.jsInstance);
        /// <inheritdoc/>
        public virtual async Task<string[]?> Neighbors(string v) => await this.jsInstance.InvokeAsync<string[]?>("neighbors", v);
        /// <inheritdoc/>
        public virtual async Task<GraphLibNode> Node(string name) => await this.jsInstance.InvokeAsync<GraphLibNode>("node", name);
        /// <inheritdoc/>
        public virtual async Task<double> NodeCount() => await this.jsInstance.InvokeAsync<double>("nodeCount");
        /// <inheritdoc/>
        public virtual async Task<GraphLibEdge[]?> NodeEdges(string v, string w) => await this.jsInstance.InvokeAsync<GraphLibEdge[]?>("nodeEdges", v, w);
        /// <inheritdoc/>
        public virtual async Task<string[]> Nodes() => await this.jsInstance.InvokeAsync<string[]>("nodes");
        /// <inheritdoc/>
        public virtual async Task<GraphLibEdge[]?> OutEdges(string v, string w) => await this.jsInstance.InvokeAsync<GraphLibEdge[]?>("outEdges", v, w);
        /// <inheritdoc/>
        public virtual async Task<string[]?> Parent(string v) => await this.jsInstance.InvokeAsync<string[]?>("parent", v);
        /// <inheritdoc/>
        public virtual async Task<string[]?> Predecessors(string v) => await this.jsInstance.InvokeAsync<string[]?>("predecessors", v);
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> RemoveEdge(GraphLibEdge edge) => await this.jsInstance.InvokeAsync<IJSObjectReference>("removeEdge", edge) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> RemoveEdge(string v, string w, string name) => await this.jsInstance.InvokeAsync<IJSObjectReference>("removeEdge", v, w, name) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> RemoveNode(string name) => await this.jsInstance.InvokeAsync<IJSObjectReference>("removeNode", name) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetDefaultEdgeLabel(object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultEdgeLabel", label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetDefaultEdgeLabel(Func<string, object> labelFn) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultEdgeLabel", labelFn) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetDefaultNodeLabel(object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultNodeLabel", label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetDefaultNodeLabel(Func<string, object> labelFn) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultNodeLabel", labelFn) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetGraph(IDagreGraphConfig label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setGraph", label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetEdge(string v, string w) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setEdge", v, w) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetEdge(string v, string w, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setEdge", v, w, label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetEdge(string v, string w, object label, string name) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setEdge", v, w, label, name) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetEdge(GraphLibEdge edge, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setEdge", edge, label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetNode(string name, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setNode", name, label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetNodes(string[] names, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setNodes", names, label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetParent(string v, string p) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setParent", v, p) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<IGraphLib> SetPath(string[] nodes, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setPath", nodes, label) as IGraphLib;
        /// <inheritdoc/>
        public virtual async Task<string[]> Sinks() => await this.jsInstance.InvokeAsync<string[]>("sinks");
        /// <inheritdoc/>
        public virtual async Task<string[]> Sources() => await this.jsInstance.InvokeAsync<string[]>("sources");
        /// <inheritdoc/>
        public virtual async Task<string[]?> Successors(string v) => await this.jsInstance.InvokeAsync<string[]?>("successors", v);
    }
}
