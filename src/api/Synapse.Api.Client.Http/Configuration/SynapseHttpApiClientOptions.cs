namespace Synapse.Api.Client.Http.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse HTTP API client
/// </summary>
public class SynapseHttpApiClientOptions
{

    /// <summary>
    /// Initializes a new <see cref="SynapseHttpApiClientOptions"/>
    /// </summary>
    public SynapseHttpApiClientOptions()
    {
        var uri = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Uri);
        this.BaseAddress = string.IsNullOrWhiteSpace(uri) ? null! : new(uri, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Gets/sets the base address of the Cloud Streams API to connect to
    /// </summary>
    public virtual Uri BaseAddress { get; set; }

}
