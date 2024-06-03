using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Cli.Commands.Namespaces;

/// <summary>
/// Represents the <see cref="Command"/> used to create a new <see cref="Namespace"/>
/// </summary>
internal class CreateNamespaceCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="CreateNamespaceCommand"/>'s name
    /// </summary>
    public const string CommandName = "create";
    /// <summary>
    /// Gets the <see cref="CreateNamespaceCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Creates a new namespace.";

    /// <inheritdoc/>
    public CreateNamespaceCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.Add(new Argument<string>("name") { Description = "The name of the namespace to create." });
        this.Handler = CommandHandler.Create<string>(this.HandleAsync);
    }

    /// <summary>
    /// Handles the <see cref="CreateNamespaceCommand"/>
    /// </summary>
    /// <param name="name">The name of the namespace to create</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        await this.Api.Namespaces.CreateAsync(new() 
        { 
            Metadata = new()
            {
                Name = name
            }
        });
        Console.WriteLine($"namespace/{name} created");
    }

}
