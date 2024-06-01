using Microsoft.Extensions.Options;
using Synapse.Cli.Configuration;
using Synapse.Cli.Services;

namespace Synapse.Cli.Commands.Config;

/// <summary>
/// Represents the <see cref="Command"/> used to configure the CLI to use the specified API configuration
/// </summary>
internal class UseApiCommand
    : Command
{

    /// <summary>
    /// Gets the <see cref="UseApiCommand"/>'s name
    /// </summary>
    public const string CommandName = "use-api";
    /// <summary>
    /// Gets the <see cref="UseApiCommand"/>'s description
    /// </summary>
    public const string CommandDescription = "Configures the API used by the Synapse CLI";

    /// <inheritdoc/>
    public UseApiCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, IOptionsManager optionsManager, IOptionsMonitor<ApplicationOptions> applicationOptions)
        : base(serviceProvider, loggerFactory, api, CommandName, CommandDescription)
    {
        this.OptionsManager = optionsManager;
        this.ApplicationOptions = applicationOptions;
        this.Add(new Argument<string>("name") { Description = "The name of the API configuration to use." });
        this.Handler = CommandHandler.Create<string>(HandleAsync);
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
    /// Handles the <see cref="UseApiCommand"/>
    /// </summary>
    /// <param name="name">The name of the API configuration to use</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task HandleAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!this.ApplicationOptions.CurrentValue.Api.Configurations.TryGetValue(name, out var apiConfig) || apiConfig == null) throw new NullReferenceException($"Failed to find a configured API with name '{name}'.");
        this.ApplicationOptions.CurrentValue.Api.Current = name;
        await this.OptionsManager.UpdateOptionsAsync(this.ApplicationOptions.CurrentValue);
    }

}