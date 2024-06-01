namespace Synapse.Cli.Configuration;

/// <summary>
/// Represents the object used to configure the Synapse API used by the CLI
/// </summary>
[DataContract]
public record ApplicationApiOptions
{

    /// <summary>
    /// Gets/sets a name/value mapping of all configured APIs
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Name = "configurations", Order = 1), JsonPropertyOrder(1), JsonPropertyName("configurations"), YamlMember(Alias = "configurations", Order = 1)]
    public virtual IDictionary<string, ApiConfiguration> Configurations { get; set; } = new Dictionary<string, ApiConfiguration>();

    /// <summary>
    /// Gets/sets the name of the API currently used by the Synapse CLI
    /// </summary>
    [DataMember(Name = "current", Order = 2), JsonPropertyOrder(2), JsonPropertyName("current"), YamlMember(Alias = "current", Order = 2)]
    public string? Current { get; set; }

}
