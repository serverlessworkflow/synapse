using Synapse.Cli.Commands.Config;

namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the <see cref="Command"/> used to configure the Synapse CLI
/// </summary>
public class ConfigCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="ConfigCommand"/>'s name
    /// </summary>
    public const string CommandName = "config";
    /// <summary>
    /// Gets the <see cref="ConfigCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Configures the Synapse CLI";

    /// <inheritdoc/>
    public ConfigCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.AddCommand(ActivatorUtilities.CreateInstance<DeleteApiCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<GetApisCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<SetApiCommand>(this.ServiceProvider));
        this.AddCommand(ActivatorUtilities.CreateInstance<UseApiCommand>(this.ServiceProvider));
    }

}
