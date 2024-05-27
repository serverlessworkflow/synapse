namespace Synapse.Runtime.Services;

/// <summary>
/// Defines the fundamentals of a service used to create and run <see cref="IWorkflowProcess"/>es
/// </summary>
public interface IWorkflowRuntime
    : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Creates a new <see cref="IWorkflowProcess"/> for the specified <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="workflow">The instantiated <see cref="Workflow"/> to start a new <see cref="IWorkflowProcess"/> for</param>
    /// <param name="workflowInstance">The <see cref="WorkflowInstance"/> to create a new <see cref="IWorkflowProcess"/> for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IWorkflowProcess"/></returns>
    Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

}
