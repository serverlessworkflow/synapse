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
            string? shape = null,
            Guid? parentId = null,
            double? width = Consts.ClusterWidth,
            double? height = Consts.ClusterHeight,
            double? x = 0,
            double? y = 0,
            double? radiusX = Consts.ClusterRadius,
            double? radiusY = Consts.ClusterRadius,
            double? paddingX = Consts.ClusterPadding,
            double? paddingY = Consts.ClusterPadding,
            Type? componentType = null
        )
            : base(label, shape, parentId, width, height, x, y, radiusX, radiusY, paddingX, paddingY, componentType)
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
    }
}
