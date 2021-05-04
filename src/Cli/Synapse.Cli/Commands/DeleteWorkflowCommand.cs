using Synapse.Cli.Services;
using Synapse.Domain.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to delete a single <see cref="V1Workflow"/>
    /// </summary>
    public class DeleteWorkflowCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="DeleteWorkflowCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public DeleteWorkflowCommand(ISynapseService synapse)
            : base(synapse, "workflow", "Deletes the specified workflow.")
        {
            this.Handler = CommandHandler.Create<string, string>(this.HandleAsync);
            this.AddArgument(new Argument<string>("name") { Description = "The name of the workflow to delete (ex: myworkflow:1.0)" });
            this.Add(CommandOptions.Namespace);
        }

        /// <summary>
        /// Handles the <see cref="DeleteWorkflowCommand"/>
        /// </summary>
        /// <param name="name">The name of the workflow to delete</param>
        /// <param name="namespace">The namespace of the workflow to delete</param>
        /// <returns>A new int representing the program's return code</returns>
        public async Task<int> HandleAsync(string name, string @namespace = "synapse")
        {
            await this.Synapse.DeleteWorkflowAsync(name, @namespace);
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
