using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="EndDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class EndNodeModel
        : WorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="EndNodeModel"/>
        /// </summary>
        public EndNodeModel()
        {
            this.AddPort(PortAlignment.Top);
        }

    }

}
