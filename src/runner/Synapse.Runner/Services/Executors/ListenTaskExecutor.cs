// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Neuroglia;
using Neuroglia.Data.Expressions;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ListenTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ListenTaskExecutor(IServiceProvider serviceProvider, ILogger<ListenTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, 
    ITaskExecutorFactory executorFactory, ITaskExecutionContext<ListenTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer)
    : TaskExecutor<ListenTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
{

    /// <summary>
    /// Gets the <see cref="ListenTaskExecutor"/>'s <see cref="ICloudEventBus"/> subscription
    /// </summary>
    protected IDisposable? Subscription { get; set; }

    /// <summary>
    /// Gets the path for the specified event
    /// </summary>
    /// <param name="offset">The offset of the event to get the path for</param>
    /// <returns>The path for the specified event</returns>
    protected virtual string GetPathFor(uint offset) => $"{nameof(ListenTaskDefinition.Foreach).ToCamelCase()}/{offset - 1}/{nameof(ForTaskDefinition.Do).ToCamelCase()}";

    /// <inheritdoc/>
    protected override async Task<ITaskExecutor> CreateTaskExecutorAsync(TaskInstance task, TaskDefinition definition, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        var executor = await base.CreateTaskExecutorAsync(task, definition, contextData, arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync
        (
            _ => System.Threading.Tasks.Task.CompletedTask,
            async ex => await this.OnEventProcessingErrorAsync(executor, this.CancellationTokenSource!.Token).ConfigureAwait(false),
            async () => await this.OnEventProcessingCompletedAsync(executor, this.CancellationTokenSource!.Token).ConfigureAwait(false)
        );
        return executor;
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.Task.Definition.Foreach == null)
        {
            var context = await this.Task.CorrelateAsync(cancellationToken).ConfigureAwait(false);
            var events = this.Task.Definition.Listen.Read switch
            {
                EventReadMode.Data or EventReadMode.Raw or null => context.Events.Select(e => e.Value.Data),
                EventReadMode.Envelope => context.Events.Select(e => e.Value),
                _ => throw new NotSupportedException($"The specified event read mode '{this.Task.Definition.Listen.Read}' is not supported")
            };
            await this.SetResultAsync(events, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if(this.Task.Definition.Foreach.Do != null)
            {
                var task = await this.Task.GetSubTasksAsync(cancellationToken).OrderBy(t => t.CreatedAt).LastOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                if (task != null && task.IsOperative)
                {
                    var taskDefinition = new DoTaskDefinition()
                    {
                        Do = this.Task.Definition.Foreach.Do,
                        Metadata =
                        [
                            new(SynapseDefaults.Tasks.Metadata.PathPrefix.Name, false)
                        ]
                    };
                    var arguments = this.GetExpressionEvaluationArguments();
                    var taskExecutor = await this.CreateTaskExecutorAsync(task, taskDefinition, this.Task.ContextData, arguments, this.CancellationTokenSource!.Token).ConfigureAwait(false);
                    await taskExecutor.ExecuteAsync(this.CancellationTokenSource!.Token).ConfigureAwait(false);
                }
            }
            var events = await this.Task.StreamAsync(cancellationToken).ConfigureAwait(false);
            this.Subscription = events.SubscribeAsync(this.OnStreamingEventAsync, this.OnStreamingErrorAsync, this.OnStreamingCompletedAsync);
        }
    }

    /// <summary>
    /// Handles the streaming of a <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="e">The streamed <see cref="CloudEvent"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnStreamingEventAsync(IStreamedCloudEvent e)
    {
        if (this.Task.Definition.Foreach!.Do != null)
        {
            var taskDefinition = new DoTaskDefinition()
            {
                Do = this.Task.Definition.Foreach.Do,
                Metadata =
                [
                    new(SynapseDefaults.Tasks.Metadata.PathPrefix.Name, false)
                ]
            };
            var arguments = this.GetExpressionEvaluationArguments();
            var eventData = this.Task.Definition.Listen.Read switch
            {
                EventReadMode.Data or EventReadMode.Raw or null => e.Event.Data,
                EventReadMode.Envelope => e.Event,
                _ => throw new NotSupportedException($"The specified event read mode '{this.Task.Definition.Listen.Read}' is not supported")
            };
            if (this.Task.Definition.Foreach.Output?.As is string fromExpression) eventData = await this.Task.Workflow.Expressions.EvaluateAsync<object>(fromExpression, eventData ?? new(), arguments, this.CancellationTokenSource!.Token).ConfigureAwait(false);
            else if (this.Task.Definition.Foreach.Output?.As != null) eventData = await this.Task.Workflow.Expressions.EvaluateAsync<object>(this.Task.Definition.Foreach.Output.As!, eventData ?? new(), arguments, this.CancellationTokenSource!.Token).ConfigureAwait(false);
            if (this.Task.Definition.Foreach.Export?.As is string toExpression)
            {
                var context = (await this.Task.Workflow.Expressions.EvaluateAsync<IDictionary<string, object>>(toExpression, eventData ?? new(), arguments, this.CancellationTokenSource!.Token).ConfigureAwait(false))!;
                await this.Task.SetContextDataAsync(context, this.CancellationTokenSource!.Token).ConfigureAwait(false);
            }
            else if (this.Task.Definition.Foreach.Export?.As != null)
            {
                var context = (await this.Task.Workflow.Expressions.EvaluateAsync<IDictionary<string, object>>(this.Task.Definition.Foreach.Export.As!, eventData ?? new(), arguments, this.CancellationTokenSource!.Token).ConfigureAwait(false))!;
                await this.Task.SetContextDataAsync(context, this.CancellationTokenSource!.Token).ConfigureAwait(false);
            }
            arguments ??= new Dictionary<string, object>();
            arguments[this.Task.Definition.Foreach.Item ?? RuntimeExpressions.Arguments.Each] = eventData!;
            arguments[this.Task.Definition.Foreach.At  ?? RuntimeExpressions.Arguments.Index] = e.Offset - 1;
            var task = await this.Task.Workflow.CreateTaskAsync(taskDefinition, this.GetPathFor(e.Offset), this.Task.Input, null, this.Task, false, this.CancellationTokenSource!.Token).ConfigureAwait(false);
            var taskExecutor = await this.CreateTaskExecutorAsync(task, taskDefinition, this.Task.ContextData, arguments, this.CancellationTokenSource!.Token).ConfigureAwait(false);
            await taskExecutor.ExecuteAsync(this.CancellationTokenSource!.Token).ConfigureAwait(false);
        }
        await e.AckAsync(this.CancellationTokenSource!.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles an <see cref="Exception"/> that occurred while streaming events
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to handle</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnStreamingErrorAsync(Exception ex) => this.SetErrorAsync(new Error()
    {
        Type = ErrorType.Communication,
        Title = ErrorTitle.Communication,
        Status = (ushort)ErrorStatus.Communication,
        Detail = ex.Message,
        Instance = this.Task.Instance.Reference
    }, this.CancellationTokenSource!.Token);

    /// <summary>
    /// Handles the completion of the event streaming
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnStreamingCompletedAsync()
    {
        var last = await this.Task.GetSubTasksAsync(this.CancellationTokenSource!.Token).OrderBy(t => t.CreatedAt).LastOrDefaultAsync(this.CancellationTokenSource!.Token).ConfigureAwait(false);
        var output = (object?)null;
        if (last != null && !string.IsNullOrWhiteSpace(last.OutputReference)) output = (await this.Task.Workflow.Documents.GetAsync(last.OutputReference, this.CancellationTokenSource.Token).ConfigureAwait(false)).Content;
        await this.SetResultAsync(output, this.Task.Definition.Then, this.CancellationTokenSource!.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles an <see cref="Exception"/> that occurred while processing a streamed event
    /// </summary>
    /// <param name="executor">The service used to execute the event processing that has faulted</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnEventProcessingErrorAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        var error = executor.Task.Instance.Error ?? throw new NullReferenceException();
        this.Executors.Remove(executor);
        await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of an event's processing
    /// </summary>
    /// <param name="executor">The service used to execute the event processing that has completed</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnEventProcessingCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        this.Executors.Remove(executor);
        if (this.Task.ContextData != executor.Task.ContextData) await this.Task.SetContextDataAsync(executor.Task.ContextData, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) this.Subscription?.Dispose();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing) this.Subscription?.Dispose();
        base.Dispose(disposing);
    }

}
