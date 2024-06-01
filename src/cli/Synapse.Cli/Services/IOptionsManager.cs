using Synapse.Cli.Configuration;

namespace Synapse.Cli.Services;

/// <summary>
/// Defines the service used to manage the application's options
/// </summary>
public interface IOptionsManager
{

    /// <summary>
    /// Updates the application's options
    /// </summary>
    /// <param name="options">The updated <see cref="ApplicationOptions"/> to persist</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task UpdateOptionsAsync(ApplicationOptions options, CancellationToken cancellationToken = default);

}

/// <summary>
/// Represents the default implementation of the <see cref="IOptionsManager"/> interface
/// </summary>
/// <param name="serializer">The service used to serialize/deserialize data to/from YAML</param>
public class OptionsManager(IYamlSerializer serializer)
    : IOptionsManager
{

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    protected IYamlSerializer Serializer { get; } = serializer;

    /// <inheritdoc/>
    public virtual async Task UpdateOptionsAsync(ApplicationOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var yaml = this.Serializer.SerializeToText(options);
        await File.WriteAllTextAsync(CliConstants.ConfigurationFileName, yaml, cancellationToken);
    }
}
