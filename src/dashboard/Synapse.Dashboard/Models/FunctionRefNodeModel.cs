using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="FunctionReference"/> <see cref="NodeModel"/>
    /// </summary>
    public class FunctionRefNodeModel
        : NodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="FunctionRefNodeModel"/>
        /// </summary>
        /// <param name="function">The <see cref="FunctionReference"/> the <see cref="FunctionRefNodeModel"/> represents</param>
        public FunctionRefNodeModel(FunctionReference function)
        {
            this.Function = function;
            this.Title = function.RefName;
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the <see cref="FunctionReference"/> the <see cref="FunctionRefNodeModel"/> represents
        /// </summary>
        public FunctionReference Function { get; }

    }

}
