using Synapse.Cli.Services;
using Synapse.Domain.Models;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to delete <see cref="V1Workflow"/>s and <see cref="V1WorkflowInstance"/>s
    /// </summary>
    public class DeleteCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="DeleteCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public DeleteCommand(ISynapseService synapse)
            : base(synapse, "delete", "Deletes the specified workflow or workflow instance.")
        {
            this.AddAlias("del");
            this.AddCommand(new DeleteWorkflowCommand(this.Synapse));
            this.AddCommand(new DeleteAllWorkflowsCommand(this.Synapse));
            this.AddCommand(new DeleteWorkflowInstanceCommand(this.Synapse));
            this.AddCommand(new DeleteAllWorkflowInstancesCommand(this.Synapse));
        }

    }

}
