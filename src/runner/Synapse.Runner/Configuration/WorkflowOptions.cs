namespace Synapse.Runner.Configuration;

/// <summary>
/// Represents the options used to configure the workflow the Synapse Runner must run and how
/// </summary>
public class WorkflowOptions
{

    /// <summary>
    /// Initializes a new <see cref="WorkflowOptions"/>
    /// </summary>
    public WorkflowOptions()
    {
        this.Instance = Environment.GetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Workflow.Instance)!;
    }

    /// <summary>
    /// Gets/sets the qualified name of the workflow instance to run
    /// </summary>
    public virtual string Instance { get; set; }

    /// <summary>
    /// Gets the namespace of the workflow instance to run
    /// </summary>
    /// <returns>The namespace of the workflow instance to run</returns>
    public virtual string GetInstanceNamespace() => this.Instance.Split('.', StringSplitOptions.RemoveEmptyEntries).First();

    /// <summary>
    /// Gets the name of the workflow instance to run
    /// </summary>
    /// <returns>The name of the workflow instance to run</returns>
    public virtual string GetInstanceName() => this.Instance.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();

}