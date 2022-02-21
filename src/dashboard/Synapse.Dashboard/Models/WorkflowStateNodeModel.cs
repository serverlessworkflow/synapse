using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{

    /// <summary>
    /// Represents a <see cref="StateDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class WorkflowStateNodeModel
        : NodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowStateNodeModel"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="WorkflowStateNodeModel"/> represents</param>
        public WorkflowStateNodeModel(StateDefinition state)
        {
            this.State = state;
            this.Title = state.Name;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> the <see cref="WorkflowStateNodeModel"/> represents
        /// </summary>
        public StateDefinition State { get; }

    }

}
