using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    public class GraphLib
        : IGraphLib
    {
        protected readonly IJSObjectReference jsInstance;

        public GraphLib(IJSObjectReference jsInstance)
        {
            this.jsInstance = jsInstance;
        }

        public virtual async Task<string[]> Children(string v) => await this.jsInstance.InvokeAsync<string[]>("children", v);

        public virtual async Task<IGraphLib> FilterNodes(Func<string, bool> filter) => await this.jsInstance.InvokeAsync<IJSObjectReference>("filterNodes", filter) as IGraphLib;

        public virtual async Task<string?> Graph() => await this.jsInstance.InvokeAsync<string?>("graph");

        public virtual async Task<bool> HasEdge(string v, string w, string name) => await this.jsInstance.InvokeAsync<bool>("hasEdge", v, w, name);

        public virtual async Task<bool> HasEdge(IGraphLibEdge edge) => await this.jsInstance.InvokeAsync<bool>("hasEdge", edge);

        public virtual async Task<bool> HasNode(string name) => await this.jsInstance.InvokeAsync<bool>("hasNode", name);

        public virtual async Task<object> Edge(string v, string w, string name) => await this.jsInstance.InvokeAsync<object>("edge", v, w, name);

        public virtual async Task<object> Edge(IGraphLibEdge e) => await this.jsInstance.InvokeAsync<object>("edge", e);

        public virtual async Task<double> EdgeCount() => await this.jsInstance.InvokeAsync<double>("edgeCount");

        public virtual async Task<IGraphLibEdge[]> Edges() => await this.jsInstance.InvokeAsync<IJSObjectReference[]>("edges") as IGraphLibEdge[];

        public virtual async Task<IGraphLibEdge[]?> InEdges(string v, string w) => await this.jsInstance.InvokeAsync<IJSObjectReference[]?>("inEdges", v, w) as IGraphLibEdge[];

        public virtual async Task<bool> IsCompound() => await this.jsInstance.InvokeAsync<bool>("isCompound");

        public virtual async Task<bool> IsDirected() => await this.jsInstance.InvokeAsync<bool>("isDirected");

        public virtual async Task<bool> IsMultigraph() => await this.jsInstance.InvokeAsync<bool>("isMultigraph");

        public virtual async Task<string[]?> Neighbors(string v) => await this.jsInstance.InvokeAsync<string[]?>("neighbors", v);

        public virtual async Task<object> Node(string name) => await this.jsInstance.InvokeAsync<object>("node", name);

        public virtual async Task<double> NodeCount() => await this.jsInstance.InvokeAsync<double>("nodeCount");

        public virtual async Task<IGraphLibEdge[]?> NodeEdges(string v, string w) => await this.jsInstance.InvokeAsync<IJSObjectReference[]?>("nodeEdges", v, w) as IGraphLibEdge[];

        public virtual async Task<string[]> Nodes() => await this.jsInstance.InvokeAsync<string[]>("nodes");

        public virtual async Task<IGraphLibEdge[]?> OutEdges(string v, string w) => await this.jsInstance.InvokeAsync<IJSObjectReference[]?>("outEdges", v, w) as IGraphLibEdge[];

        public virtual async Task<string[]?> Parent(string v) => await this.jsInstance.InvokeAsync<string[]?>("parent", v);

        public virtual async Task<string[]?> Predecessors(string v) => await this.jsInstance.InvokeAsync<string[]?>("predecessors", v);

        public virtual async Task<IGraphLib> RemoveEdge(IGraphLibEdge edge) => await this.jsInstance.InvokeAsync<IJSObjectReference>("removeEdge", edge) as IGraphLib;

        public virtual async Task<IGraphLib> RemoveEdge(string v, string w, string name) => await this.jsInstance.InvokeAsync<IJSObjectReference>("removeEdge", v, w, name) as IGraphLib;

        public virtual async Task<IGraphLib> RemoveNode(string name) => await this.jsInstance.InvokeAsync<IJSObjectReference>("removeNode", name) as IGraphLib;

        public virtual async Task<IGraphLib> SetDefaultEdgeLabel(object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultEdgeLabel", label) as IGraphLib;

        public virtual async Task<IGraphLib> SetDefaultEdgeLabel(Func<string, object> labelFn) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultEdgeLabel", labelFn) as IGraphLib;

        public virtual async Task<IGraphLib> SetDefaultNodeLabel(object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultNodeLabel", label) as IGraphLib;

        public virtual async Task<IGraphLib> SetDefaultNodeLabel(Func<string, object> labelFn) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setDefaultNodeLabel", labelFn) as IGraphLib;

        public virtual async Task<IGraphLib> SetGraph(string label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setGraph", label) as IGraphLib;

        public virtual async Task<IGraphLib> SetEdge(string v, string w, object label, string name) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setEdge", v, w, label, name) as IGraphLib;

        public virtual async Task<IGraphLib> SetEdge(IGraphLibEdge edge, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setEdge", edge, label) as IGraphLib;

        public virtual async Task<IGraphLib> SetNode(string name, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setNode", name, label) as IGraphLib;

        public virtual async Task<IGraphLib> SetNodes(string[] names, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setNodes", names, label) as IGraphLib;

        public virtual async Task<IGraphLib> SetParent(string v, string p) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setParent", v, p) as IGraphLib;

        public virtual async Task<IGraphLib> SetPath(string[] nodes, object label) => await this.jsInstance.InvokeAsync<IJSObjectReference>("setPath", nodes, label) as IGraphLib;

        public virtual async Task<string[]> Sinks() => await this.jsInstance.InvokeAsync<string[]>("sinks");

        public virtual async Task<string[]> Sources() => await this.jsInstance.InvokeAsync<string[]>("sources");

        public virtual async Task<string[]?> Successors(string v) => await this.jsInstance.InvokeAsync<string[]?>("successors", v);
    }
}
