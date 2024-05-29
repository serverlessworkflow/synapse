using Synapse.Api.Client.Http.Configuration;

namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Runner application
/// </summary>
public class RunnerOptions
{

    /// <summary>
    /// Gets/sets the options used to configure the Synapse API the runner must use
    /// </summary>
    public virtual SynapseHttpApiClientOptions Api { get; set; } = new();

    /// <summary>
    /// Gets/sets the options used to configure the workflow the Synapse Runner must run and how
    /// </summary>
    public virtual WorkflowOptions Workflow { get; set; } = new();

}
