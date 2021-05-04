using Synapse.Cli.Services;
using Synapse.Domain.Models;
using System.CommandLine;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to delete a single <see cref="V1WorkflowInstance"/>
    /// </summary>
    public class DeleteWorkflowInstanceCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="DeleteWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public DeleteWorkflowInstanceCommand(ISynapseService synapse)
            : base(synapse, "workflow-instance", "Deletes the specified workflow instance.")
        {
            this.AddAlias("instance");
            this.AddArgument(new Argument<string>("name") { Description = "The name of the workflow instance to delete" });
        }

    }

}
