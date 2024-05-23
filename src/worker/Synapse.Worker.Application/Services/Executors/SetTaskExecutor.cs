using Neuroglia.Data.Expressions;

namespace Synapse.Worker.Application.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="SetTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class SetTaskExecutor(IServiceProvider serviceProvider, ILogger<SetTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<SetTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<SetTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var input = await this.Task.Workflow.Expressions.EvaluateAsync<object>(this.Task.Definition.Set, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken: cancellationToken).ConfigureAwait(false);
        await this.SetResultAsync(input, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}