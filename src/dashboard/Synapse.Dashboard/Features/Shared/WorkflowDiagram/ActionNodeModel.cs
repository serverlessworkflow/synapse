using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="ActionDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class ActionNodeModel
        : WorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="ActionNodeModel"/>
        /// </summary>
        /// <param name="action">The <see cref="ActionDefinition"/> the <see cref="ActionNodeModel"/> represents</param>
        public ActionNodeModel(ActionDefinition action)
        {
            this.Action = action;
            this.Title = action.Name!;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> the <see cref="ActionNodeModel"/> represents
        /// </summary>
        public ActionDefinition Action { get; }

    }

}
