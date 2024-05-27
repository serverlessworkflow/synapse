namespace Synapse.Runtime.Configuration;

/// <summary>
/// Represents the options used to configure the Synapse API to use
/// </summary>
public class ApiOptions
{

    /// <summary>
    /// Gets/sets the URI that references the Synapse API to use
    /// </summary>
    public required Uri Uri { get; set; }

} 
