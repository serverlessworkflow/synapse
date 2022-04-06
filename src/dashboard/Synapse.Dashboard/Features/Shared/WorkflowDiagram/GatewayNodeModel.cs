using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a logical gateway <see cref="NodeModel"/>
    /// </summary>
    public class GatewayNodeModel
        : WorkflowNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="GatewayNodeModel"/>
        /// </summary>
        /// <param name="completionType">The gateway's ParallelCompletionType</param>
        public GatewayNodeModel(ParallelCompletionType completionType)
        {
            this.CompletionType = completionType;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the gateway's ParallelCompletionType
        /// </summary>
        public ParallelCompletionType CompletionType { get; }

    }
    

}
