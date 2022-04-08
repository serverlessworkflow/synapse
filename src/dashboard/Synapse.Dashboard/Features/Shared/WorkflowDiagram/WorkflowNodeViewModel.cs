using Neuroglia.Blazor.Dagre;
using Neuroglia.Blazor.Dagre.Models;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents the base class for all workflow-related <see cref="NodeModel"/>s
    /// </summary>
    public abstract class WorkflowNodeViewModel
        : NodeViewModel, IWorkflowNodeViewModel
    {

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> ActiveInstances { get; } = new();

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> FaultedInstances { get; } = new();

        public WorkflowNodeViewModel(
            string? label = "",
            string? cssClass = null,
            string? shape = null,
            double? width = Consts.NodeWidth,
            double? height = Consts.NodeHeight,
            double? radiusX = Consts.NodeRadius,
            double? radiusY = Consts.NodeRadius,
            double? x = 0,
            double? y = 0,
            Type? componentType = null,
            Guid? parentId = null
        )
            : base(label, cssClass, shape, width, height, radiusX, radiusY, x, y, componentType, parentId)
        {
            this.ActiveInstances.CollectionChanged += this.OnCollectionChanged;
        }

        protected void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //this.Changed?.Invoke();
        }
    }

}
