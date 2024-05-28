namespace Synapse.Api.Client.Http.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse HTTP API client
/// </summary>
public class SynapseHttpApiClientOptions
{

    /// <summary>
    /// Gets/sets the base address of the Cloud Streams API to connect to
    /// </summary>
    public required virtual Uri BaseAddress { get; set; }

}
