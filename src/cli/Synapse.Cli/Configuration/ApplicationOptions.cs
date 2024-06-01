namespace Synapse.Cli.Configuration;

/// <summary>
/// Represents the options used to configure the Synapse CLI application
/// </summary>
[DataContract]
public record ApplicationOptions
{

    /// <summary>
    /// Gets/sets the name of the API currently used by the Synapse CLI
    /// </summary>
    [DataMember(Name = "api", Order = 1), JsonPropertyOrder(1), JsonPropertyName("api"), YamlMember(Alias = "api", Order = 1)]
    public ApplicationApiOptions Api { get; set; } = new();

}
