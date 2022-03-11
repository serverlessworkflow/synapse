using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents the base class for all workflow-related <see cref="NodeModel"/>s
    /// </summary>
    public abstract class WorkflowNodeModel
        : NodeModel, IWorkflowNodeModel
    {

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> ActiveInstances { get; } = new();

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> FaultedInstances { get; } = new();

        public WorkflowNodeModel(Point? position = null, RenderLayer layer = RenderLayer.HTML, ShapeDefiner? shape = null)
            : base(position, layer, shape)
        {
            this.ActiveInstances.CollectionChanged += this.OnCollectionChanged;
        }

        public WorkflowNodeModel(string id, Point? position = null, RenderLayer layer = RenderLayer.HTML, ShapeDefiner? shape = null)
            : base(id, position, layer, shape)
        {
            this.ActiveInstances.CollectionChanged += this.OnCollectionChanged;
        }

        protected void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this.Refresh();
        }
    }

}
