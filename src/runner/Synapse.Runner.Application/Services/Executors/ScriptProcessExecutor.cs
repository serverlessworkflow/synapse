namespace Synapse.Runner.Application.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="RunTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ScriptProcessExecutor(IServiceProvider serviceProvider, ILogger<ScriptProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<RunTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the definition of the script process to run
    /// </summary>
    protected ScriptProcessDefinition ProcessDefinition => this.Task.Definition.Run.Script!;

    /// <inheritdoc/>
    protected override Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        return System.Threading.Tasks.Task.CompletedTask; //todo: implement
    }

}
