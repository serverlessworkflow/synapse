namespace Synapse.Runtime.Configuration;

/// <summary>
/// Represents the base class for all runtime options
/// </summary>
public abstract class RuntimeOptions
{

    /// <summary>
    /// Gets/sets the options used to configure the API used by processes created by the configured runtime
    /// </summary>
    public required ApiOptions Api { get; set; }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to skip certificate validation
    /// </summary>
    public bool SkipCertificateValidation { get; set; }

}
