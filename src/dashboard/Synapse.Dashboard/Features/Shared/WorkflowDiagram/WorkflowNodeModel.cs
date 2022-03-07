using Blazor.Diagrams.Core.Models;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents the base class for all workflow-related <see cref="NodeModel"/>s
    /// </summary>
    public abstract class WorkflowNodeModel
        : NodeModel, IWorkflowNodeModel
    {

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstanceDto> ActiveInstances { get; } = new();

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstanceDto> FaultedInstances { get; } = new();

    }

}
