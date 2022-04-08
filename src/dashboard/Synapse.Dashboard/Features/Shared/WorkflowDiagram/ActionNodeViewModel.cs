using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Represents a <see cref="ActionDefinition"/> <see cref="NodeModel"/>
    /// </summary>
    public class ActionNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="ActionNodeViewModel"/>
        /// </summary>
        /// <param name="action">The <see cref="ActionDefinition"/> the <see cref="ActionNodeViewModel"/> represents</param>
        public ActionNodeViewModel(ActionDefinition action, string? cssClass = "action-node")
            : base(action.Name ?? action.Function?.RefName ?? action.Subflow?.WorkflowId ?? action.RetryRef, cssClass)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> the <see cref="ActionNodeViewModel"/> represents
        /// </summary>
        public ActionDefinition Action { get; }

    }

}
