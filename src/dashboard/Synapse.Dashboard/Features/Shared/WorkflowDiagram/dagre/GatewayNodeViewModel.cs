using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a logical gateway <see cref="NodeViewModel"/>
    /// </summary>
    public class GatewayNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="GatewayNodeViewModel"/>
        /// </summary>
        /// <param name="completionType">The gateway's ParallelCompletionType</param>
        public GatewayNodeViewModel(ParallelCompletionType completionType)
        {
            this.CompletionType = completionType;
        }

        /// <summary>
        /// Gets the gateway's ParallelCompletionType
        /// </summary>
        public ParallelCompletionType CompletionType { get; }

    }
    

}
