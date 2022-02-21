using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="StartDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class WorkflowStartNodeModel
        : NodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowStateNodeModel"/>
        /// </summary>
        public WorkflowStartNodeModel()
        {
            this.AddPort(PortAlignment.Bottom);
        }

    }

}
