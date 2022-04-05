using System.Collections.ObjectModel;

namespace Neuroglia.Blazor.Dagre.Models
{
    public class ClusterViewModel
        : NodeViewModel, IClusterViewModel
    {
        protected readonly Dictionary<Guid, INodeViewModel> _children;
        public virtual IReadOnlyDictionary<Guid, INodeViewModel> Children => this._children;

        protected readonly Dictionary<Guid, INodeViewModel> _allNodes;
        public virtual IReadOnlyDictionary<Guid, INodeViewModel> AllNodes => this._allNodes;

        protected readonly Dictionary<Guid, IClusterViewModel> _allClusters;
        public virtual IReadOnlyDictionary<Guid, IClusterViewModel> AllClusters => this._allClusters;

        public ClusterViewModel(
            Dictionary<Guid, INodeViewModel>? children = null,
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Consts.ClusterWidth,
            double? height = Consts.ClusterHeight,
            double? radiusX = Consts.ClusterRadius,
            double? radiusY = Consts.ClusterRadius,
            double? x = 0,
            double? y = 0,
            Type? componentType = null,
            Guid? parentId = null
        )
            : base(label, cssClass, shape, width, height, radiusX, radiusY, x, y, componentType, parentId)
        {
            this._children = children ?? new Dictionary<Guid, INodeViewModel>();
            this._allNodes = new Dictionary<Guid, INodeViewModel>();
            this._allClusters = new Dictionary<Guid, IClusterViewModel>();
            foreach(var child in this._children.Values)
            {
                if (child == null)
                {
                    continue;
                }
                child.ParentId = this.Id;
                child.Changed += OnChildChanged;
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

        public override void Move (double deltaX, double deltaY)
        {
            if (deltaX == 0 && deltaY == 0)
                return;
            base.Move(deltaX, deltaY);
            foreach(var child in this._children.Values)
            {
                child.Move(deltaX, deltaY);
            }
        }

        /// <summary>
        /// Adds the provided cluster to the graph
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task AddCluster(IClusterViewModel cluster)
        {
            if (cluster == null)
            {
                throw new ArgumentNullException(nameof(cluster));
            }
            cluster.ParentId = this.Id;
            this._children.Add(cluster.Id, cluster);
            this._allClusters.Add(cluster.Id, cluster);
            this.Flatten(cluster);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Adds the provided node to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task AddNode(INodeViewModel node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (node is IClusterViewModel cluster)
            {
                await this.AddCluster(cluster);
                return;
            }
            node.ParentId = this.Id;
            this._children.Add(node.Id, node);
            this._allNodes.Add(node.Id, node);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Adds nested nodes/clusters to allNodes/Clusters
        /// </summary>
        /// <param name="cluster"></param>
        protected virtual void Flatten(IClusterViewModel cluster)
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
   
        protected virtual void OnChildChanged()
        {
            var minX = this._allNodes.Values.Select(node => node.X - (node.Width ?? 0) / 2).Min();
            var maxX = this._allNodes.Values.Select(node => node.X + (node.Width ?? 0) / 2).Max();
            var minY = this._allNodes.Values.Select(node => node.Y - (node.Height ?? 0) / 2).Min();
            var maxY = this._allNodes.Values.Select(node => node.Y + (node.Height ?? 0) / 2).Max();
            var centerX = this.Children.Values.Average(child => child.X);
            var centerY = this.Children.Values.Average(child => child.Y);
            this.X = centerX;
            this.Y = centerY;
            this.Width = maxX - minX + Consts.ClusterPaddingX;
            this.Height = maxY - minY + Consts.ClusterPaddingY;
        }
    }
}
