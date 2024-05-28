namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Runner application
/// </summary>
public class RunnerOptions
{

    /// <summary>
    /// Gets/sets the options used to configure the workflow the Synapse Runner must run and how
    /// </summary>
    public required virtual WorkflowOptions Workflow { get; set; }

}
