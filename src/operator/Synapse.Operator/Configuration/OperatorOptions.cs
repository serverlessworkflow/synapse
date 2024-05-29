namespace Synapse.Operator.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Operator application
/// </summary>
public class OperatorOptions
{

    /// <summary>
    /// Initializes a new <see cref="OperatorOptions"/>
    /// </summary>
    public OperatorOptions()
    {
        Namespace = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Namespace)!;
        Name = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Operator.Name)!;
    }

    /// <summary>
    /// Gets/sets the operator's namespace
    /// </summary>
    public virtual string Namespace { get; set; }

    /// <summary>
    /// Gets/sets the operator's name
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Gets/sets the options used to configure the runners spawned by a Synapse Operator
    /// </summary>
    public virtual RunnerDefinition Runner { get; set; } = new();

}
