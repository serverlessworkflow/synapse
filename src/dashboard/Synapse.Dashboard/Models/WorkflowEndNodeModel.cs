using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="EndDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class WorkflowEndNodeModel
        : NodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowEndNodeModel"/>
        /// </summary>
        public WorkflowEndNodeModel()
        {
            this.AddPort(PortAlignment.Top);
        }

    }

}
