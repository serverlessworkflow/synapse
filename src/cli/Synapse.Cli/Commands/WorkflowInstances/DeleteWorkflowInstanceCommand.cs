namespace Synapse.Cli.Commands.WorkflowInstances;

/// <summary>
/// Represents the <see cref="Command"/> used to delete a single <see cref="WorkflowInstance"/>
/// </summary>
internal class DeleteWorkflowInstanceCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="DeleteWorkflowInstanceCommand"/>'s name
    /// </summary>
    public const string CommandName = "delete";
    /// <summary>
    /// Gets the <see cref="DeleteWorkflowInstanceCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Deletes the specified workflow instance";

    /// <inheritdoc/>
    public DeleteWorkflowInstanceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("del");
        this.Add(new Argument<string>("name") { Description = "The name of the workflow instance to delete" });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Confirm);
        this.Handler = CommandHandler.Create<string, string, bool>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="DeleteWorkflowInstanceCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace of the workflow to delete</param>
    /// <param name="name">The name of the workflow to delete</param>
    /// <param name="y">A boolean indicating whether or not to ask for the user's confirmation</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, string @namespace, bool y)
    {
        if (!y)
        {
            Console.Write($"Are you sure you wish to delete the workflow instance '{name}.{@namespace}'? Press 'y' to confirm, or any other key to cancel: ");
            var inputKey = Console.ReadKey();
            Console.WriteLine();
            if (inputKey.Key != ConsoleKey.Y) return;
        }
        await this.Api.WorkflowInstances.DeleteAsync(name, @namespace);
        Console.WriteLine($"workflow-instance/{name} deleted");
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new([ "-n", "--namespace" ], () => "default", "The namespace the workflow instance to delete belongs to.");

        public static Option<bool> Confirm => new(["-y", "--yes"], () => false, "Delete the workflow instance without prompting confirmation.");

    }

}
