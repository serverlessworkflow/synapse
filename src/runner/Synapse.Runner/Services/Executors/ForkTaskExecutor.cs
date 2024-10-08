﻿// Copyright © 2024-Present The Synapse Authors
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
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ForkTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="ISchemaHandler"/> implementations</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ForkTaskExecutor(IServiceProvider serviceProvider, ILogger<ForkTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<ForkTaskDefinition> context, ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer)
    : TaskExecutor<ForkTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
{

    /// <summary>
    /// Gets the path to the specified subtask
    /// </summary>
    /// <param name="index">The index of the subtask to get the path to</param>
    /// <param name="name">The name of the subtask to get the path to</param>
    /// <returns>The path to the specified subtask</returns>
    protected virtual string GetPathFor(int index, string name) => $"{nameof(ForkTaskDefinition.Fork).ToCamelCase()}/{nameof(BranchingDefinition.Branches).ToCamelCase()}/{index}/{name}";

    /// <inheritdoc/>
    protected override async Task<ITaskExecutor> CreateTaskExecutorAsync(TaskInstance task, TaskDefinition definition, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        var executor = await base.CreateTaskExecutorAsync(task, definition, contextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync(
            _ => System.Threading.Tasks.Task.CompletedTask,
            async ex => await this.OnSubTaskFaultAsync(executor, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false),
            async () => await this.OnSubTaskCompletedAsync(executor, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false), this.CancellationTokenSource?.Token ?? default);
        return executor;
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var tasks = this.Task.GetSubTasksAsync(cancellationToken);
        tasks = await tasks.AnyAsync(cancellationToken).ConfigureAwait(false)
            ? tasks.Where(t => t.IsOperative)
            : this.Task.Definition.Fork.Branches
                .ToAsyncEnumerable()
                .SelectAwait(async (kvp, index) => await this.Task.Workflow.CreateTaskAsync(kvp.Value, this.GetPathFor(index, kvp.Key), this.Task.Input, null, this.Task, false, cancellationToken).ConfigureAwait(false));
        await System.Threading.Tasks.Task.WhenAll(await tasks
            .SelectAwait(async task =>
            {
                var definition = this.Task.Workflow.Definition.GetComponent<TaskDefinition>(task.Reference.OriginalString);
                var executor = await this.CreateTaskExecutorAsync(task, definition, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
                return executor.ExecuteAsync(cancellationToken);
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false));
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
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        var error = executor.Task.Instance.Error ?? throw new NullReferenceException();
        this.Executors.Remove(executor);
        foreach (var subExecutor in this.Executors)
        {
            await subExecutor.CancelAsync(cancellationToken).ConfigureAwait(false);
            this.Executors.Remove(executor);
        }
        await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of a subtask's execution
    /// </summary>
    /// <param name="executor">The service used to execute sub tasks</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnSubTaskCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        using var @lock = await this.Lock.LockAsync(cancellationToken).ConfigureAwait(false);
        if (this.Task.Instance.Status != TaskInstanceStatus.Running)
        {
            if (this.Executors.Remove(executor)) await executor.CancelAsync(cancellationToken).ConfigureAwait(false);
        }
        if (this.Task.Definition.Fork.Compete == true)
        {
            var output = executor.Task.Output!;
            foreach (var concurrentTaskExecutor in this.Executors)
            {
                this.Executors.Remove(concurrentTaskExecutor);
                await concurrentTaskExecutor.CancelAsync(cancellationToken).ConfigureAwait(false);
            }
            await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            this.Executors.Remove(executor);
            await executor.DisposeAsync().ConfigureAwait(false);
            var subTasks = this.Task.GetSubTasksAsync(cancellationToken);
            if (await subTasks.AllAsync(t => t.Status == TaskInstanceStatus.Skipped || t.Status == TaskInstanceStatus.Completed || t.Status == TaskInstanceStatus.Cancelled || t.Status == TaskInstanceStatus.Faulted, cancellationToken).ConfigureAwait(false))
            {
                var result = new { };
                await this.SetResultAsync(result, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
            }
        }
    }

}