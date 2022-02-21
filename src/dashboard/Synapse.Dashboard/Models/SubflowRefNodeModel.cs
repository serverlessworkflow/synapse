using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Models
{
    /// <summary>
    /// Represents a <see cref="SubflowReference"/> <see cref="NodeModel"/>
    /// </summary>
    public class SubflowRefNodeModel
        : NodeModel
    {

        /// <summary>
        /// Initializes a new <see cref="SubflowRefNodeModel"/>
        /// </summary>
        /// <param name="subflow">The <see cref="SubflowReference"/> the <see cref="SubflowRefNodeModel"/> represents</param>
        public SubflowRefNodeModel(SubflowReference subflow)
        {
            this.Subflow = subflow;
            this.Title = $"{subflow.WorkflowId}{(string.IsNullOrEmpty(subflow.Version) ? "" : $":{subflow.Version}")}";
            this.AddPort(PortAlignment.Top);
            this.AddPort(PortAlignment.Bottom);
        }

        /// <summary>
        /// Gets the <see cref="SubflowReference"/> the <see cref="SubflowRefNodeModel"/> represents
        /// </summary>
        public SubflowReference Subflow { get; }

    }

}
