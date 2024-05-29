using Synapse.Core.Infrastructure.Services;

namespace Synapse.Runtime.Services;

/// <summary>
/// Represents the native implementation of the <see cref="IWorkflowRuntime"/>
/// </summary>
/// <remarks>
/// Initializes a new <see cref="ContainerRuntime"/>
/// </remarks>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="environment">The current <see cref="IHostEnvironment"/></param>
/// <param name="options">The service used to access the current <see cref="RunnerDefinition"/></param>
/// <param name="containerPlatform">The service used to manage containers</param>
public class ContainerRuntime(ILoggerFactory loggerFactory, IHostEnvironment environment, IContainerPlatform containerPlatform, IOptionsMonitor<RunnerDefinition> options)
    : WorkflowRuntimeBase(loggerFactory)
{

    /// <summary>
    /// Gets the current <see cref="IHostEnvironment"/>
    /// </summary>
    protected IHostEnvironment Environment { get; } = environment;

    /// <summary>
    /// Gets the service used to manage containers
    /// </summary>
    protected IContainerPlatform ContainerPlatform { get; } = containerPlatform;

    /// <summary>
    /// Gets the current <see cref="RunnerDefinition"/>
    /// </summary>
    protected RunnerDefinition Options => options.CurrentValue;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all known runner processes
    /// </summary>
    protected ConcurrentDictionary<string, ContainerProcess> Processes { get; } = new();

    /// <inheritdoc/>
    public override async Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(workflowInstance);
        var containerDefinition = this.Options.Runtime.Container!.Clone()!;
        containerDefinition.Environment ??= [];
        containerDefinition.Environment[SynapseDefaults.EnvironmentVariables.Api.Uri] = this.Options.Api.Uri.OriginalString;
        containerDefinition.Environment[SynapseDefaults.EnvironmentVariables.Workflow.Instance] = workflowInstance.GetQualifiedName();
        var container = await this.ContainerPlatform.CreateAsync(containerDefinition, cancellationToken).ConfigureAwait(false);
        return this.Processes.AddOrUpdate(workflowInstance.GetQualifiedName(), new ContainerProcess(container), (key, current) => current);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing) return;
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        base.Dispose(disposing);
    }

}
