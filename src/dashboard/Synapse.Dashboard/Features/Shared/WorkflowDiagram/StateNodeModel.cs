using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Integration.Models;
using System.Collections.ObjectModel;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="StateDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class StateNodeModel
        : GroupModel, IWorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="StateNodeModel"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="StateNodeModel"/> represents</param>
        public StateNodeModel(StateDefinition state)
            : base(Array.Empty<NodeModel>())
        {
            this.State = state;
            this.Title = state.Name!;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
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
