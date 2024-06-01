namespace Synapse.Cli.Configuration;

/// <summary>
/// Represents the named options used to configure a Synapse API to connect to using the Synapse CLI
/// </summary>
[DataContract]
public class ApiConfiguration
{

    /// <summary>
    /// Gets/sets the uri that references the API server to connect to
    /// </summary>
    [DataMember(Name = "server", Order = 1), JsonPropertyOrder(1), JsonPropertyName("server"), YamlMember(Alias = "server", Order = 1)]
    public required virtual Uri Server { get; set; }

}