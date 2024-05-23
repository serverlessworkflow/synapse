using Neuroglia.Data.Expressions;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Eventing.CloudEvents;
using Neuroglia.Eventing.CloudEvents.Infrastructure.Services;

namespace Synapse.Worker.Application.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="EmitTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="cloudEventBus">The service used to stream both input and output <see cref="CloudEvent"/>s</param>
public class EmitTaskExecutor(IServiceProvider serviceProvider, ILogger<EmitTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, 
    ITaskExecutorFactory executorFactory, ITaskExecutionContext<EmitTaskDefinition> context, IJsonSerializer serializer, ICloudEventBus cloudEventBus)
    : TaskExecutor<EmitTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to stream both input and output <see cref="CloudEvent"/>s
    /// </summary>
    protected ICloudEventBus CloudEventBus { get; } = cloudEventBus;

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var attributes = this.Task.Definition.Emit.Event.With.Clone() ?? [];
        if(!attributes.ContainsKey(CloudEventAttributes.Id)) attributes[CloudEventAttributes.Id] = Guid.NewGuid().ToString();
        if (!attributes.ContainsKey(CloudEventAttributes.SpecVersion)) attributes[CloudEventAttributes.SpecVersion] = CloudEventSpecVersion.V1.Version;
        if (!attributes.ContainsKey(CloudEventAttributes.Time)) attributes[CloudEventAttributes.Time] = DateTimeOffset.Now;
        var e = (await this.Task.Workflow.Expressions.EvaluateAsync<CloudEvent>(attributes, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))!;
        this.CloudEventBus.OutputStream.OnNext(e);
        await this.SetResultAsync(e, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}
