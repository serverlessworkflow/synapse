using Synapse.Cli.Services;
using Synapse.Domain.Models;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to delete all <see cref="V1WorkflowInstance"/>s
    /// </summary>
    public class DeleteAllWorkflowInstancesCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="DeleteAllWorkflowInstancesCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public DeleteAllWorkflowInstancesCommand(ISynapseService synapse)
            : base(synapse, "workflow-instances", "Deletes all workflow instances.")
        {
            this.AddAlias("instances");
        }

    }

}
