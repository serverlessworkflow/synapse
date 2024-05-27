namespace Synapse.Runtime.Services;

/// <summary>
/// Represents the base class for all <see cref="IWorkflowRuntime"/> implementations
/// </summary>
public abstract class WorkflowRuntimeBase
    : IWorkflowRuntime
{

    bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="WorkflowRuntimeBase"/>
    /// </summary>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    protected WorkflowRuntimeBase(ILoggerFactory loggerFactory)
    {
        this.Logger = loggerFactory.CreateLogger(this.GetType());
    }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <inheritdoc/>
    public abstract Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disposes of the <see cref="WorkflowProcessBase"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowProcessBase"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed) return;
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="WorkflowProcessBase"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowProcessBase"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed) return;

        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}
