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

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="DoTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class DoTaskExecutor(IServiceProvider serviceProvider, ILogger<DoTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<DoTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer)
    : TaskExecutor<DoTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
{

    /// <summary>
    /// Gets a name/definition mapping of the tasks to execute
    /// </summary>
    protected Map<string, TaskDefinition> Tasks => this.Task.Definition.Do ?? throw new NullReferenceException("The task must define at least two tasks to perform sequentially");

    /// <summary>
    /// Gets the path to the specified subtask
    /// </summary>
    /// <param name="index">The index of the subtask to get the path to</param>
    /// <param name="name">The name of the subtask to get the path to</param>
    /// <returns>The path to the specified subtask</returns>
    protected virtual string GetPathFor(int index, string name) => this.Task.Definition.Extensions?.TryGetValue(SynapseDefaults.Tasks.ExtensionProperties.PathPrefix.Name, out var value) == true && value is bool prefix && prefix == false
        ? $"{index}/{name}"
        : $"{nameof(DoTaskDefinition.Do).ToCamelCase()}/{index}/{name}";

    /// <inheritdoc/>
    protected override async Task<ITaskExecutor> CreateTaskExecutorAsync(TaskInstance task, TaskDefinition definition, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        var executor = await base.CreateTaskExecutorAsync(task, definition, contextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync(
            _ => System.Threading.Tasks.Task.CompletedTask,
            async ex => await this.OnSubTaskFaultAsync(executor, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false),
            async () => await this.OnSubtaskCompletedAsync(executor, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false), this.CancellationTokenSource?.Token ?? default);
        return executor;
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var last = await this.Task.GetSubTasksAsync(cancellationToken).LastOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var nextDefinition = last == null
            ? this.Tasks.First()
            : last.IsOperative
                ? this.Task.Definition.Do.FirstOrDefault(e => e.Key == last.Name) ?? throw new NullReferenceException($"Failed to find a task with the specified name '{last.Name}' at '{this.Task.Instance.Reference}'")
                : this.Task.Definition.Do.GetTaskAfter(last);
        if (nextDefinition == null)
        {
            await this.SetResultAsync(last!.OutputReference, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
            return;
        }
        var nextDefinitionIndex = this.Task.Definition.Do.Keys.ToList().IndexOf(nextDefinition.Key);
        var input = last == null ? this.Task.Input : (await this.Task.Workflow.Documents.GetAsync(last.OutputReference!, cancellationToken).ConfigureAwait(false))!;
        var next = await this.Task.Workflow.CreateTaskAsync(nextDefinition.Value, this.GetPathFor(nextDefinitionIndex, nextDefinition.Key), input, null, this.Task, this.Task.Instance.IsExtension, cancellationToken).ConfigureAwait(false);
        var executor = await this.CreateTaskExecutorAsync(next, nextDefinition.Value, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles error raised during a subtask's execution
    /// </summary>
    /// <param name="executor">The service used to execute sub tasks</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnSubTaskFaultAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        var error = executor.Task.Instance.Error ?? throw new NullReferenceException();
        this.Executors.Remove(executor);
        await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of a subtask's execution
    /// </summary>
    /// <param name="executor">The service used to execute sub tasks</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnSubtaskCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        var last = executor.Task.Instance;
        var output = executor.Task.Output!;
        var nextDefinition = this.Task.Definition.Do.GetTaskAfter(last);
        this.Executors.Remove(executor);
        if (this.Task.ContextData != executor.Task.ContextData) await this.Task.SetContextDataAsync(executor.Task.ContextData, cancellationToken).ConfigureAwait(false);
        if (nextDefinition == null || nextDefinition.Value == null)
        {
            await this.SetResultAsync(output, last.Status != TaskInstanceStatus.Skipped && last.Next == FlowDirective.End ? FlowDirective.End : this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
            return;
        }
        var nextDefinitionIndex = this.Task.Definition.Do.Keys.ToList().IndexOf(nextDefinition.Key);
        TaskInstance next;
        switch (executor.Task.Instance.Status == TaskInstanceStatus.Skipped ? FlowDirective.Continue : executor.Task.Instance.Next)
        {
            case FlowDirective.End:
                await this.SetResultAsync(output, FlowDirective.End, cancellationToken).ConfigureAwait(false);
                break;
            case FlowDirective.Exit:
                await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
                break;
            default:
                next = await this.Task.Workflow.CreateTaskAsync(nextDefinition.Value, this.GetPathFor(nextDefinitionIndex, nextDefinition.Key), output, null, this.Task, false, cancellationToken).ConfigureAwait(false);
                var nextExecutor = await this.CreateTaskExecutorAsync(next, nextDefinition.Value, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
                await nextExecutor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                break;
        }
    }

}
