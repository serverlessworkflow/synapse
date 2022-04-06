using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="StateDefinition"/> <see cref="NodeViewModel"/>
    /// </summary>
    public class StateNodeViewModel
        : ClusterViewModel, IWorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="StateNodeViewModel"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="StateNodeViewModel"/> represents</param>
        public StateNodeViewModel(StateDefinition state)
            : base(null, state.Name!)
        {
            this.State = state;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> the <see cref="ActionNodeModel"/> represents
        /// </summary>
        public StateDefinition State { get; }

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> ActiveInstances { get; } = new();

        /// <inheritdoc/>
        public ObservableCollection<V1WorkflowInstance> FaultedInstances { get; } = new();

    }

}
