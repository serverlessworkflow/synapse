namespace Synapse.Cli.Commands;

/// <summary>
/// Represents the base class for all <see cref="System.CommandLine.Command"/> implementations
/// </summary>
public abstract class Command
    : System.CommandLine.Command
{

    /// <summary>
    /// Initializes a new <see cref="Command"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    /// <param name="api">The service used to interact with the remote Synapse API</param>
    /// <param name="name">The <see cref="Command"/>'s name</param>
    /// <param name="description">The <see cref="Command"/>'s description</param>
    protected Command(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseApiClient api, string name, string description)
        : base(name, description)
    {
        this.ServiceProvider = serviceProvider;
        this.Logger = loggerFactory.CreateLogger(this.GetType());
        this.Api = api;
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the service used to interact with the remote Synapse API
    /// </summary>
    protected ISynapseApiClient Api { get; }

}
