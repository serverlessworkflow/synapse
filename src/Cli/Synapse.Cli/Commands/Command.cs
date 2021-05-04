using Synapse.Cli.Services;
using System.CommandLine.Invocation;
using System.Reflection;

namespace Synapse.Cli.Commands
{

    public abstract class Command
        : System.CommandLine.Command
    {

        protected Command(ISynapseService synapse, string name, string description) 
            : base(name, description)
        {
            this.Synapse = synapse;
        }

        protected ISynapseService Synapse { get; }

        protected virtual void CreateHandler(string name = "HandleAsync")
        {
            this.Handler = CommandHandler.Create(this.GetType().GetMethod(name, BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static), this);
        }

    }

}
