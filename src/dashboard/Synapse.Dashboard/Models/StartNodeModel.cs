using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="StartDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class StartNodeModel
        : WorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowStateNodeModel"/>
        /// </summary>
        public StartNodeModel()
        {
            this.AddPort(PortAlignment.Bottom);
        }

    }

}
