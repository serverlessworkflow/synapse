using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Neuroglia.Blazor.Dagre.Behaviors;
using Neuroglia.Blazor.Dagre.Templates;
using System.Collections.ObjectModel;

namespace Neuroglia.Blazor.Dagre.Models
{
    public class GraphViewModel
        : GraphElement, IGraphViewModel
    {
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual IGraphLib? DagreGraph { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? X { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Y { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Width { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public virtual double? Height { get; set; }
        public virtual decimal Scale { get; set; }

        /// <summary>
        /// The first level graph nodes (direct children)
        /// </summary>
        protected readonly Dictionary<Guid, INodeViewModel> _nodes;
        public virtual IReadOnlyDictionary<Guid, INodeViewModel> Nodes => this._nodes;

        /// <summary>
        /// The flattened graph nodes (nested children)
        /// </summary>
        protected readonly Dictionary<Guid, INodeViewModel> _allNodes;
        public virtual IReadOnlyDictionary<Guid, INodeViewModel> AllNodes => this._allNodes;

        /// <summary>
        /// The graph edges
        /// </summary>
        protected readonly Dictionary<Guid, IEdgeViewModel> _edges;
        public virtual IReadOnlyDictionary<Guid, IEdgeViewModel> Edges => this._edges;

        /// <summary>
        /// The first level graph clusters (direct children)
        /// </summary>
        protected readonly Dictionary<Guid, IClusterViewModel> _clusters;
        public virtual IReadOnlyDictionary<Guid, IClusterViewModel> Clusters => this._clusters;

        /// <summary>
        /// The flattened graph clusters (nested children)
        /// </summary>
        protected readonly Dictionary<Guid, IClusterViewModel> _allClusters;
        public virtual IReadOnlyDictionary<Guid, IClusterViewModel> AllClusters => this._allClusters;

        protected readonly Collection<Type> _svgDefinitionComponents;
        public virtual IReadOnlyCollection<Type> SvgDefinitionComponents => this._svgDefinitionComponents;

        public virtual bool ShowConstruction { get; set; }

        /// <summary>
        /// The map of node type and their component type
        /// </summary>
        protected readonly Dictionary<Type, Type> _components;

        private Type _defaultNodeComponentType = typeof(NodeTemplate);
        protected virtual Type DefaultNodeComponentType => this._defaultNodeComponentType;

        private Type _defaultClusterComponentType = typeof(ClusterTemplate);
        protected virtual Type DefaultClusterComponentType => this._defaultClusterComponentType;

        private Type _defaultEdgeComponentType = typeof(EdgeTemplate);
        protected virtual Type DefaultEdgeComponentType => this._defaultEdgeComponentType;

        private readonly Dictionary<Type, GraphBehavior> _behaviors;

        public event MouseEventHandler? MouseMove;
        public event MouseEventHandler? MouseDown;
        public event MouseEventHandler? MouseUp;
        public event MouseEventHandler? MouseEnter;
        public event MouseEventHandler? MouseLeave;
        public event WheelEventHandler? Wheel;

        public GraphViewModel(
            Dictionary<Guid, INodeViewModel>? nodes = null,
            Dictionary<Guid, IEdgeViewModel>? edges = null,
            Dictionary<Guid, IClusterViewModel>? clusters = null,
            Collection<Type>? svgDefinitions = null,
            string? cssClass = null,
            double? width = null,
            double? height = null,
            string? label = null,
            Type? componentType = null,
            bool showConstruction = false
        )
            : base(label, cssClass, componentType)
        {
            this._nodes = nodes ?? new Dictionary<Guid, INodeViewModel>();
            this._edges = edges ?? new Dictionary<Guid, IEdgeViewModel>(); ;
            this._clusters = clusters ?? new Dictionary<Guid, IClusterViewModel>();
            this._svgDefinitionComponents = svgDefinitions ?? new Collection<Type>() {
                typeof(ArrowDefinitionTemplate)
            };
            this.Scale = 1;
            this.X = 0;
            this.Y = 0;
            this.Width = width;
            this.Height = height;
            this._components = new Dictionary<Type, Type>();
            this._allNodes = new Dictionary<Guid, INodeViewModel>();
            this._allClusters = new Dictionary<Guid, IClusterViewModel>();
            this._behaviors = new Dictionary<Type, GraphBehavior>();
            this.ShowConstruction = showConstruction;
            // this.RegisterBehavior(new DebugEventsBehavior(this));
            this.RegisterBehavior(new ZoomBahavior(this));
            this.RegisterBehavior(new PanBahavior(this));
            // this.RegisterBehavior(new MoveNodeBehavior(this));
            foreach (var node in this._nodes.Values)
            {
                if (node == null)
                {
                    continue;
                }
                this._allNodes.Add(node.Id, node);
            }
            foreach (var cluster in this._clusters.Values)
            {
                if (cluster == null)
                {
                    continue;
                }
                cluster.ChildAdded += this.OnChildAdded;
                this._allClusters.Add(cluster.Id, cluster);
                this.Flatten(cluster);
            }
        }


        /// <summary>
        /// Adds the provided <see cref="IGraphElement"/> to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task AddElementAsync(IGraphElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (element is IEdgeViewModel edge)
            {
                await this.AddEdgeAsync(edge);
                return;
            }
            if (element is IClusterViewModel cluster)
            {
                await this.AddClusterAsync(cluster);
                return;
            }
            if (element is INodeViewModel node)
            {
                await this.AddNodeAsync(node);
                return;
            }
            throw new Exception("Unknown element type");
        }

        /// <summary>
        /// Adds the provided <see cref="IGraphElement"/>s to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task AddElementsAsync(IEnumerable<IGraphElement> elements)
        {
            if (elements == null || !elements.Any())
            {
                throw new ArgumentNullException(nameof(elements));
            }
            foreach (var element in elements)
            {
                await this.AddElementAsync(element);
            }
        }

        /// <summary>
        /// Adds the provided <see cref="IClusterViewModel"/> to the graph
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual async Task AddClusterAsync(IClusterViewModel cluster)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException(nameof(cluster));
            }
            this._clusters.Add(cluster.Id, cluster);
            this._allClusters.Add(cluster.Id, cluster);
            cluster.ChildAdded += this.OnChildAdded;
            this.Flatten(cluster);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Adds the provided <see cref="INodeViewModel"/> to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual async Task AddNodeAsync(INodeViewModel node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (node is IClusterViewModel cluster)
            {
                await this.AddClusterAsync(cluster);
                return;
            }
            this._nodes.Add(node.Id, node);
            this._allNodes.Add(node.Id, node);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Adds the provided <see cref="IEdgeViewModel"/> to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual async Task AddEdgeAsync(IEdgeViewModel edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }
            this._edges.Add(edge.Id, edge);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Registers a component type associated with a node type
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public virtual async Task RegisterComponentTypeAsync<TElement, TComponent>()
            where TElement : IGraphElement
            where TComponent : ComponentBase
        {
            var elementType = typeof(TElement);
            var componentType = typeof(TComponent);
            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(TElement));
            }
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(TComponent));
            }
            if (this._components.ContainsKey(elementType))
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }
            this._components.Add(elementType, componentType);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the component type associated with the specified node type
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual Type GetComponentTypeAsync<TElement>(TElement element)
            where TElement : IGraphElement
        {
            if (element.ComponentType != null)
            {
                return element.ComponentType;
            }
            var elementType = element.GetType();
            if (this._components.ContainsKey(elementType))
            {
                return this._components[elementType];
            }
            if (element is IClusterViewModel)
            {
                return this.DefaultClusterComponentType;
            }
            if (element is IEdgeViewModel)
            {
                return this.DefaultEdgeComponentType;
            }
            return this.DefaultNodeComponentType;
        }

        /// <summary>
        /// Adds nested nodes/clusters to allNodes/Clusters
        /// </summary>
        /// <param name="cluster"></param>
        public virtual void Flatten(IClusterViewModel cluster)
        {
            foreach (var subClusters in cluster.AllClusters.Values)
            {
                if (subClusters == null)
                {
                    continue;
                }
                this._allClusters.Add(subClusters.Id, subClusters);
            }
            foreach (var subNode in cluster.AllNodes.Values)
            {
                if (subNode == null)
                {
                    continue;
                }
                this._allNodes.Add(subNode.Id, subNode);
            }
        }

        /// <summary>
        /// Adds the provided <see cref="GraphBehavior"/>
        /// </summary>
        /// <param name="graphBehavior"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void RegisterBehavior(GraphBehavior graphBehavior)
        {
            var behaviorType = graphBehavior.GetType();
            if (this._behaviors.ContainsKey(behaviorType))
            {
                throw new ArgumentException($"A behavior of type '{behaviorType.ToString()}' already exists", nameof(graphBehavior));
            }
            this._behaviors.Add(behaviorType, graphBehavior);
        }

        /// <summary>
        /// Removes the provided <see cref="GraphBehavior"/>
        /// </summary>
        /// <param name="graphBehavior"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void UnregisterBehavior(GraphBehavior graphBehavior)
        {
            var behaviorType = graphBehavior.GetType();
            if (!this._behaviors.ContainsKey(behaviorType))
                return;
            this._behaviors[behaviorType].Dispose();
            this._behaviors.Remove(behaviorType);
        }

        public virtual async Task OnMouseMoveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this.MouseMove != null)
                await this.MouseMove.Invoke(sender, e, element);
        }

        public virtual async Task OnMouseDownAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this.MouseDown != null)
                await this.MouseDown.Invoke(sender, e, element);
        }

        public virtual async Task OnMouseUpAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this.MouseUp != null)
                await this.MouseUp.Invoke(sender, e, element);
        }

        public virtual async Task OnMouseEnterAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this.MouseEnter != null)
                await this.MouseEnter.Invoke(sender, e, element);
        }

        public virtual async Task OnMouseLeaveAsync(ElementReference sender, MouseEventArgs e, IGraphElement? element)
        {
            if (this.MouseLeave != null)
                await this.MouseLeave.Invoke(sender, e, element);
        }

        public virtual async Task OnWheelAsync(ElementReference sender, WheelEventArgs e, IGraphElement? element)
        {
            if (this.Wheel != null)
                await this.Wheel.Invoke(sender, e, element);
        }

        public virtual void OnChildAdded(INodeViewModel child)
        {
            if (child is IClusterViewModel cluster)
            {
                this._allClusters.Add(cluster.Id, cluster);
                this.Flatten(cluster);
            }
            else if (child is INodeViewModel node)
            {
                this._allNodes.Add(node.Id, node);
            }
        }
    }
}
