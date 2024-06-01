namespace Synapse.Cli.Commands.Workflows;

/// <summary>
/// Represents the <see cref="Command"/> used to delete a single <see cref="Workflow"/>
/// </summary>
internal class DeleteWorkflowCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="DeleteWorkflowCommand"/>'s name
    /// </summary>
    public const string CommandName = "delete";
    /// <summary>
    /// Gets the <see cref="DeleteWorkflowCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Deletes the specified workflow";

    /// <inheritdoc/>
    public DeleteWorkflowCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("del");
        this.Add(new Argument<string>("name") { Description = "The name of the workflow to delete" });
        this.Add(CommandOptions.Namespace);
        this.Add(CommandOptions.Version);
        this.Add(CommandOptions.Confirm);
        this.Handler = CommandHandler.Create<string, string, string, bool>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="DeleteWorkflowCommand"/>
    /// </summary>
    /// <param name="namespace">The namespace of the workflow to delete</param>
    /// <param name="name">The name of the workflow to delete</param>
    /// <param name="version">The version of the workflow to delete</param>
    /// <param name="y">A boolean indicating whether or not to ask for the user's confirmation</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, string @namespace, string version, bool y)
    {
        var components = name.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (!y)
        {
            if (components.Length == 2) Console.Write($"Are you sure you wish to delete the workflow '{name}.{@namespace}'? Press 'y' to confirm, or any other key to cancel: ");
            else Console.Write($"Are you sure you wish to delete all version of the workflow '{name}.{@namespace}'? Press 'y' to confirm, or any other key to cancel: ");
            var inputKey = Console.ReadKey();
            Console.WriteLine();
            if (inputKey.Key != ConsoleKey.Y)
            {
                Console.WriteLine("Deletion cancelled");
                return;
            }
        }
        await this.Api.Workflows.DeleteAsync(name, @namespace);
        if (components.Length == 2) Console.WriteLine($"The workflow '{name}.{@namespace}' has been successfully deleted");
        else Console.WriteLine($"All version of the workflow '{name}.{@namespace}' have been successfully deleted");
    }

    static class CommandOptions
    {

        public static Option<string> Namespace => new([ "-n", "--namespace" ], () => "default", "The namespace the workflow to delete belongs to");

        public static Option<string> Version => new(["-v", "--version"], () => string.Empty, "The version of the workflow to delete. Note that failing to specify the version will delete all version of the specified workflow");

        public static Option<bool> Confirm => new(["-y", "--yes"], () => false, "Delete the workflow(s) without prompting confirmation");

    }

}
