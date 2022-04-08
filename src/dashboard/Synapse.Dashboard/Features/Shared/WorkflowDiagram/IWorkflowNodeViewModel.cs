using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Defines the fundamentals of a workflow node
    /// </summary>
    public interface IWorkflowNodeViewModel
    {

        /// <summary>
        /// Gets an <see cref="ObservableCollection{T}"/> containing the active <see cref="V1WorkflowInstance"/>s for which the activity described by the node is active
        /// </summary>
        ObservableCollection<V1WorkflowInstance> ActiveInstances { get; }

        /// <summary>
        /// Gets an <see cref="ObservableCollection{T}"/> containing the faulted <see cref="V1WorkflowInstance"/>s for which the activity described by the node is active
        /// </summary>
        ObservableCollection<V1WorkflowInstance> FaultedInstances { get; }

    }

}
