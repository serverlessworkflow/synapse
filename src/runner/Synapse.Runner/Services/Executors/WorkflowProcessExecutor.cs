namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="WorkflowProcessDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class WorkflowProcessExecutor(IServiceProvider serviceProvider, ILogger<WorkflowProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<RunTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the definition of the shell process to run
    /// </summary>
    protected WorkflowProcessDefinition ProcessDefinition => this.Task.Definition.Run.Workflow!;

    /// <inheritdoc/>
    protected override Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

}