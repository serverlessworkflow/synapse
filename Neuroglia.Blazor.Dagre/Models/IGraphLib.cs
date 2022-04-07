using Microsoft.JSInterop;

namespace Neuroglia.Blazor.Dagre.Models
{
    /// <summary>
    /// Provides data structures for undirected and directed multi-graphs
    /// </summary>
    public interface IGraphLib
        : IMetadata
    {

        /// <summary>
        /// Gets list of direct children of node v.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// children nodes names list.
        /// </returns>
        Task<string[]> ChildrenAsync(string v);

        /// <summary>
        /// Gets the label for the specified edge.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// value associated with specified edge.
        /// </returns>
        Task<GraphLibEdge> EdgeAsync(string v, string w);
        Task<GraphLibEdge> EdgeAsync(string v, string w, string name);

        /// <summary>
        /// Gets the label for the specified edge.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// value associated with specified edge.
        /// </returns>
        Task<GraphLibEdge> EdgeAsync(GraphLibEdge e);

        /// <summary>
        /// Gets the number of edges in the graph.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// edges count.
        /// </returns>
        Task<double> EdgeCountAsync();

        /// <summary>
        /// Gets edges of the graph. In case of compound graph subgraphs are not considered.
        /// Complexity: O(|E|).
        /// </summary>
        /// <returns>
        /// graph edges list.
        /// </returns>
        Task<GraphLibEdge[]> EdgesAsync();

        /// <summary>
        /// Creates new graph with nodes filtered via filter. Edges incident to rejected node
        /// are also removed. In case of compound graph, if parent is rejected by filter,
        /// than all its children are rejected too.
        /// Average-case complexity: O(|E|+|V|).
        /// </summary>
        /// <returns>
        /// new graph made from current and nodes filtered.
        /// </returns>
        Task<IGraphLib> FilterNodesAsync(Func<string, bool> filter);

        /// <summary>
        /// Gets the graph metadata.
        /// </summary>
        /// <returns>
        /// currently assigned label for the graph or undefined if no label assigned.
        /// </returns>
        Task<IDagreGraphConfig?> GraphAsync();

        /// <summary>
        /// Detects whether the graph contains specified edge or not. No subgraphs are considered.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// whether the graph contains the specified edge or not.
        /// </returns>
        Task<bool> HasEdgeAsync(string v, string w, string name);

        /// <summary>
        /// Detects whether the graph contains specified edge or not. No subgraphs are considered.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// whether the graph contains the specified edge or not.
        /// </returns>
        Task<bool> HasEdgeAsync(GraphLibEdge edge);

        /// <summary>
        /// Detects whether graph has a node with specified name or not.
        /// </summary>
        /// <returns>
        /// true if graph has node with specified name, false - otherwise.
        /// </returns>
        Task<bool> HasNodeAsync(string name);

        /// <summary>
        /// Return all edges that point to the node v. Optionally filters those edges down to just those
        /// coming from node u. Behavior is undefined for undirected graphs - use nodeEdges instead.
        /// Complexity: O(|E|).
        /// </summary>
        /// <returns>
        /// edges descriptors list if v is in the graph, or undefined otherwise.
        /// </returns>
        Task<GraphLibEdge[]?> InEdgesAsync(string v, string w);

        /// <summary>
        /// Whether graph was created with 'compound' flag set to true or not.
        /// </summary>
        /// <returns>
        /// whether a node of the graph can have subnodes.
        /// </returns>
        Task<bool> IsCompoundAsync();

        /// <summary>
        /// Whether graph was created with 'directed' flag set to true or not.
        /// </summary>
        /// <returns>
        /// whether the graph edges have an orientation.
        /// </returns>
        Task<bool> IsDirectedAsync();

        /// <summary>
        /// Whether graph was created with 'multigraph' flag set to true or not.
        /// </summary>
        /// <returns>
        /// whether the pair of nodes of the graph can have multiple edges.
        /// </returns>
        Task<bool> IsMultigraphAsync();

        /// <summary>
        /// Gets the underlying <see cref="IJSObjectReference"/> instance
        /// </summary>
        /// <returns></returns>
        Task<IJSObjectReference> InstanceAsync();

        /// <summary>
        /// Return all nodes that are predecessors or successors of the specified node or undefined if
        /// node v is not in the graph.
        /// Complexity: O(|V|).
        /// </summary>
        /// <returns>
        /// node identifiers list or undefined if v is not in the graph.
        /// </returns>
        Task<string[]?> NeighborsAsync(string v);

        /// <summary>
        /// Gets the label of node with specified name.
        /// Complexity: O(|V|).
        /// </summary>
        /// <returns>
        /// label value of the node.
        /// </returns>
        Task<GraphLibNode> NodeAsync(string name);

        /// <summary>
        /// Gets the number of nodes in the graph.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// nodes count.
        /// </returns>
        Task<double> NodeCountAsync();

        /// <summary>
        /// Returns all edges to or from node v regardless of direction. Optionally filters those edges
        /// down to just those between nodes v and w regardless of direction.
        /// Complexity: O(|E|).
        /// </summary>
        /// <returns>
        /// edges descriptors list if v is in the graph, or undefined otherwise.
        /// </returns>
        Task<GraphLibEdge[]?> NodeEdgesAsync(string v, string w);

        /// <summary>
        /// Gets all nodes of the graph. Note, the in case of compound graph subnodes are
        /// not included in list.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// list of graph nodes.
        /// </returns>
        Task<string[]> NodesAsync();

        /// <summary>
        /// Return all edges that are pointed at by node v. Optionally filters those edges down to just
        /// those point to w. Behavior is undefined for undirected graphs - use nodeEdges instead.
        /// Complexity: O(|E|).
        /// </summary>
        /// <returns>
        /// edges descriptors list if v is in the graph, or undefined otherwise.
        /// </returns>
        Task<GraphLibEdge[]?> OutEdgesAsync(string v, string w);

        /// <summary>
        /// Gets parent node for node v.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// parent node name or void if v has no parent.
        /// </returns>
        Task<string[]?> ParentAsync(string v);

        /// <summary>
        /// Return all nodes that are predecessors of the specified node or undefined if node v is not in
        /// the graph. Behavior is undefined for undirected graphs - use neighbors instead.
        /// Complexity: O(|V|).
        /// </summary>
        /// <returns>
        /// node identifiers list or undefined if v is not in the graph.
        /// </returns>
        Task<string[]?> PredecessorsAsync(string v);

        /// <summary>
        /// Removes the specified edge from the graph. No subgraphs are considered.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> RemoveEdgeAsync(GraphLibEdge edge);

        /// <summary>
        /// Removes the specified edge from the graph. No subgraphs are considered.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> RemoveEdgeAsync(string v, string w, string name);

        /// <summary>
        /// Remove the node with the name from the graph or do nothing if the node is not in
        /// the graph. If the node was removed this function also removes any incident
        /// edges.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> RemoveNodeAsync(string name);

        /// <summary>
        /// Sets the default edge label. This label will be assigned as default label
        /// in case if no label was specified while setting an edge.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetDefaultEdgeLabelAsync(object label);

        /// <summary>
        /// Sets the default edge label factory function. This function will be invoked
        /// each time when setting an edge with no label specified and returned value
        /// will be used as a label for edge.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetDefaultEdgeLabelAsync(Func<string, object> labelFn);

        /// <summary>
        /// Sets the default node label. This label will be assigned as default label
        /// in case if no label was specified while setting a node.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetDefaultNodeLabelAsync(object label);

        /// <summary>
        /// Sets the default node label factory function. This function will be invoked
        /// each time when setting a node with no label specified and returned value
        /// will be used as a label for node.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetDefaultNodeLabelAsync(Func<string, object> labelFn);

        /// <summary>
        /// Creates or updates the label for the edge (v, w) with the optionally supplied
        /// name. If label is supplied it is set as the value for the edge. If label is not
        /// supplied and the edge was created by this call then the default edge label will
        /// be assigned. The name parameter is only useful with multigraphs.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetEdgeAsync(string v, string w);
        Task<IGraphLib> SetEdgeAsync(string v, string w, object label);
        Task<IGraphLib> SetEdgeAsync(string v, string w, object label, string name);

        /// <summary>
        /// Creates or updates the label for the specified edge. If label is supplied it is
        /// set as the value for the edge. If label is not supplied and the edge was created
        /// by this call then the default edge label will be assigned. The name parameter is
        /// only useful with multigraphs.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetEdgeAsync(GraphLibEdge edge, object label);

        /// <summary>
        /// Sets the metadata of the graph.
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetGraphAsync(IDagreGraphConfig label);

        /// <summary>
        /// Creates or updates the value for the node v in the graph. If label is supplied
        /// it is set as the value for the node. If label is not supplied and the node was
        /// created by this call then the default node label will be assigned.
        /// Complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetNodeAsync(string name, object label);

        /// <summary>
        /// Invokes setNode method for each node in names list.
        /// Complexity: O(|names|).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetNodesAsync(string[] names, object label);

        /// <summary>
        /// Sets node p as a parent for node v if it is defined, or removes the
        /// parent for v if p is undefined. Method throws an exception in case of
        /// invoking it in context of noncompound graph.
        /// Average-case complexity: O(1).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetParentAsync(string v, string p);

        /// <summary>
        /// Establish an edges path over the nodes in nodes list. If some edge is already
        /// exists, it will update its label, otherwise it will create an edge between pair
        /// of nodes with label provided or default label if no label provided.
        /// Complexity: O(|nodes|).
        /// </summary>
        /// <returns>
        /// the graph, allowing this to be chained with other functions.
        /// </returns>
        Task<IGraphLib> SetPathAsync(string[] nodes, object label);

        /// <summary>
        /// Gets list of nodes without out-edges.
        /// Complexity: O(|V|).
        /// </summary>
        /// <returns>
        /// the graph source nodes.
        /// </returns>
        Task<string[]> SinksAsync();

        /// <summary>
        /// Gets list of nodes without in-edges.
        /// Complexity: O(|V|).
        /// </summary>
        /// <returns>
        /// the graph source nodes.
        /// </returns>
        Task<string[]> SourcesAsync();

        /// <summary>
        /// Return all nodes that are successors of the specified node or undefined if node v is not in
        /// the graph. Behavior is undefined for undirected graphs - use neighbors instead.
        /// Complexity: O(|V|).
        /// </summary>
        /// <returns>
        /// node identifiers list or undefined if v is not in the graph.
        /// </returns>
        Task<string[]?> SuccessorsAsync(string v);
    }
}
