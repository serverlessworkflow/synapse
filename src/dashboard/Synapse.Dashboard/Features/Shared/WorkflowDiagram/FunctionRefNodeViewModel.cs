using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a <see cref="FunctionReference"/> <see cref="ActionNodeModel"/>
    /// </summary>
    public class FunctionRefNodeViewModel
        : ActionNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="FunctionRefNodeViewModel"/>
        /// </summary>
        /// <param name="action">The <see cref="ActionDefinition"/> the <see cref="FunctionRefNodeViewModel"/> represents</param>
        /// <param name="function">The <see cref="FunctionReference"/> the <see cref="FunctionRefNodeViewModel"/> represents</param>
        public FunctionRefNodeViewModel(ActionDefinition action, FunctionReference function) 
            : base(action, "function-node")
        {
            this.Function = function;
        }

        /// <summary>
        /// Gets the <see cref="FunctionReference"/> the <see cref="FunctionRefNodeViewModel"/> represents
        /// </summary>
        public FunctionReference Function { get; }

    }

}
