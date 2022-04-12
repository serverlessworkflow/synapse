using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a <see cref="SubflowReference"/> <see cref="NodeViewModel"/>
    /// </summary>
    public class SubflowRefNodeViewModel
        : LabeledNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="SubflowRefNodeViewModel"/>
        /// </summary>
        /// <param name="subflow">The <see cref="SubflowReference"/> the <see cref="SubflowRefNodeViewModel"/> represents</param>
        public SubflowRefNodeViewModel(SubflowReference subflow)
            : base($"{subflow.WorkflowId}{(string.IsNullOrEmpty(subflow.Version) ? "" : $":{subflow.Version}")}", "subflow-node")
        {
            this.Subflow = subflow;
        }

        /// <summary>
        /// Gets the <see cref="SubflowReference"/> the <see cref="SubflowRefNodeViewModel"/> represents
        /// </summary>
        public SubflowReference Subflow { get; }

    }

}
