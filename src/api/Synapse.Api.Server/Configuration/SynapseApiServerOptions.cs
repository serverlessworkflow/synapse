namespace Synapse.Api.Server.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse API server
/// </summary>
public class SynapseApiServerOptions
{

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to serve the Synapse Dashboard
    /// </summary>
    public bool ServeDashboard { get; set; } = true;

}
