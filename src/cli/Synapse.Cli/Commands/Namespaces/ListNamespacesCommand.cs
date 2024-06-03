using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Namespaces;

/// <summary>
/// Represents the <see cref="Command"/> used to list<see cref="Namespace"/>s
/// </summary>
internal class ListNamespacesCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ListNamespacesCommand"/>'s name
    /// </summary>
    public const string CommandName = "list";
    /// <summary>
    /// Gets the <see cref="ListNamespacesCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Lists namespaces";

    /// <inheritdoc/>
    public ListNamespacesCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddAlias("ls");
        this.Handler = CommandHandler.Create(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="ListNamespacesCommand"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync()
    {
        var table = new Table();
        var isEmpty = true;
        table.Border(TableBorder.None);
        table.AddColumn("NAME");
        table.AddColumn("CREATED AT", column =>
        {
            column.Alignment = Justify.Center;
        });
        await foreach (var @namespace in await this.Api.Namespaces.ListAsync())
        {
            isEmpty = false;
            table.AddRow
            (
                @namespace.GetName(),
                @namespace.Metadata.CreationTimestamp.ToString()!
            );
        }
        if (isEmpty)
        {
            AnsiConsole.WriteLine($"No resource found");
            return;
        }
        AnsiConsole.Write(table);
    }

}
