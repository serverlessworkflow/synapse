using Synapse.Cli.Services;
using System.CommandLine;
using System.Threading.Tasks;

namespace Synapse.Cli.Commands
{
    /// <summary>
    /// Represents the <see cref="Command"/> used to install Synapse
    /// </summary>
    public class InstallCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="InstallCommand"/>
        /// </summary>
        /// <param name="synapse">The service used to interact with Synapse</param>
        public InstallCommand(ISynapseService synapse)
            : base(synapse, "install", "Installs Synapse.")
        {
            this.CreateHandler();
            this.AddOption(CommandOptions.Namespace);
        }

        /// <summary>
        /// Handles an <see cref="InstallCommand"/>
        /// </summary>
        /// <param name="namespace">The namespace to install Synapse into. Defaults to 'synapse'</param>
        /// <returns>A new int representing the program's return code</returns>
        protected virtual async Task<int> HandleAsync(string @namespace = "synapse")
        {
            await this.Synapse.InstallAsync(@namespace);
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
                    option.Description = "The namespace to install Synapse into. Defaults to 'synapse'";
                    return option;
                }
            }

        }

    }

}
