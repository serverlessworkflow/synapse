using Microsoft.Extensions.Options;
using Synapse.Cli.Configuration;
using Synapse.Cli.Services;

namespace Synapse.Cli.Commands.Config;

/// <summary>
/// Represents the <see cref="Command"/> used to configure the API used by the Synapse CLI
/// </summary>
internal class SetApiCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="SetApiCommand"/>'s name
    /// </summary>
    public const string CommandName = "set-api";
    /// <summary>
    /// Gets the <see cref="SetApiCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Configures the API used by the Synapse CLI";

    /// <inheritdoc/>
    public SetApiCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IOptionsManager optionsManager, IOptionsMonitor<ApplicationOptions> applicationOptions)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.OptionsManager = optionsManager;
        this.ApplicationOptions = applicationOptions;
        this.Add(new Argument<string>("name") { Description = "The name of the API configuration to update." });
        this.Add(CommandOptions.Server);
        this.Handler = CommandHandler.Create<string, Uri>(HandleAsync);
    }

    /// <summary>
    /// Gets the service used to manage the application's options
    /// </summary>
    protected IOptionsManager OptionsManager { get; }

    /// <summary>
    /// Gets the current <see cref="ApplicationOptions"/>
    /// </summary>
    protected IOptionsMonitor<ApplicationOptions> ApplicationOptions { get; }

    /// <summary>
    /// Handles the <see cref="SetApiCommand"/>
    /// </summary>
    /// <param name="name">The name of the API configuration to update</param>
    /// <param name="server">The uri of the API server to use</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name, Uri server)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(server);
        if (!this.ApplicationOptions.CurrentValue.Api.Configurations.TryGetValue(name, out var apiConfig) || apiConfig == null) apiConfig = new ApiConfiguration()
        {
            Server = server
        };
        apiConfig.Server = server;
        this.ApplicationOptions.CurrentValue.Api.Configurations[name] = apiConfig;
        if (this.ApplicationOptions.CurrentValue.Api.Configurations.Count == 1) this.ApplicationOptions.CurrentValue.Api.Current = name;
        await this.OptionsManager.UpdateOptionsAsync(this.ApplicationOptions.CurrentValue);
    }

    static class CommandOptions
    {

        public static Option<Uri> Server => new(["-s", "--server"], "The address of the API server to use");

    }

}