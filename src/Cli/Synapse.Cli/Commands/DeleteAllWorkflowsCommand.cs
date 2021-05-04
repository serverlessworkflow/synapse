using Synapse.Cli.Services;
using Synapse.Domain.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to delete all <see cref="V1Workflow"/>s
    /// </summary>
    public class DeleteAllWorkflowsCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="DeleteAllWorkflowsCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public DeleteAllWorkflowsCommand(ISynapseService synapse)
            : base(synapse, "workflows", "Deletes the specified workflow.")
        {
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
            this.Add(CommandOptions.Namespace);
        }

        /// <summary>
        /// Handles the <see cref="DeleteAllWorkflowsCommand"/>
        /// </summary>
        /// <param name="namespace">The namespace the workflows to delete belong to/param>
        /// <returns>A new int representing the program's return code</returns>
        public async Task<int> HandleAsync(string @namespace = "synapse")
        {
            await this.Synapse.DeleteAllWorkflowsAsync(@namespace);
            return 0;
        }

        private static class CommandOptions
        {

            public static Option<string> Namespace
            {
                get
                {
                    Option<string> option = new Option<string>("--namespace");
                    option.AddAlias("-n");
                    option.Description = "The namespace the workflow to delete belongs to. Defaults to 'synapse'";
                    return option;
                }
            }

        }

    }

}
