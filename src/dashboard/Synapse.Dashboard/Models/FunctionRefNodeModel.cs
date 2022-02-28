using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="FunctionReference"/> <see cref="ActionNodeModel"/>
    /// </summary>
    public class FunctionRefNodeModel
        : ActionNodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="FunctionRefNodeModel"/>
        /// </summary>
        /// <param name="action">The <see cref="ActionDefinition"/> the <see cref="FunctionRefNodeModel"/> represents</param>
        /// <param name="function">The <see cref="FunctionReference"/> the <see cref="FunctionRefNodeModel"/> represents</param>
        public FunctionRefNodeModel(ActionDefinition action, FunctionReference function) 
            : base(action)
        {
            this.Function = function;
        }

        /// <summary>
        /// Gets the <see cref="FunctionReference"/> the <see cref="FunctionRefNodeModel"/> represents
        /// </summary>
        public FunctionReference Function { get; }

    }

}
