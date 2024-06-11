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
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Reactive;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ForTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ForTaskExecutor(IServiceProvider serviceProvider, ILogger<ForTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<ForTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<ForTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the path for the specified subtask
    /// </summary>
    /// <param name="subTaskName">The name of the subtask to get the path for</param>
    /// <returns></returns>
    protected virtual string GetPathFor(string subTaskName) => $"{nameof(ForTaskDefinition.For).ToCamelCase()}/{subTaskName}/{nameof(ForTaskDefinition.Do).ToCamelCase()}";


    /// <summary>
    /// Gets the items to enumerate
    /// </summary>
    protected IReadOnlyList<object>? Collection { get; set; }

    /// <inheritdoc/>
    protected override async Task<ITaskExecutor> CreateTaskExecutorAsync(TaskInstance task, TaskDefinition definition, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        var executor = await base.CreateTaskExecutorAsync(task, definition, contextData, arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync
        (
            _ => System.Threading.Tasks.Task.CompletedTask,
            async ex => await this.OnIterationFaultAsync(executor, cancellationToken).ConfigureAwait(false),
            async () => await this.OnIterationCompletedAsync(executor, cancellationToken).ConfigureAwait(false) 
        );
        return executor;
    }

    /// <inheritdoc/>
    protected override async Task DoInitializeAsync(CancellationToken cancellationToken)
    {
        this.Collection = (await this.Task.Workflow.Expressions.EvaluateAsync<List<object>>(this.Task.Definition.For.In, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))!.AsReadOnly();
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if(this.Collection == null) throw new InvalidOperationException("The executor must be initialized before execution");
        var task = await this.Task.GetSubTasksAsync(cancellationToken).OrderBy(t => t.CreatedAt).LastOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var index = task == null ? 0 : int.Parse(task.Reference.OriginalString.Split('/', StringSplitOptions.RemoveEmptyEntries).Last());
        if(index == this.Collection.Count - 1)
        {
            await this.SetResultAsync(this.Task.Input, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
            return;
        }
        var item = this.Collection.ElementAt(index);
        if (task == null) task = await this.Task.Workflow.CreateTaskAsync(this.Task.Definition.Do, this.GetPathFor("0"), this.Task.Input, null, this.Task, false, cancellationToken).ConfigureAwait(false);
        else if(!task.IsOperative) task = await this.Task.Workflow.CreateTaskAsync(this.Task.Definition.Do, this.GetPathFor($"{index + 1}"), this.Task.Input, null, this.Task, false, cancellationToken).ConfigureAwait(false);
        var contextData = this.Task.ContextData.Clone()!;
        var arguments = this.Task.Arguments.Clone()!;
        arguments[this.Task.Definition.For.Each ?? RuntimeExpressions.Arguments.Each] = item;
        arguments[this.Task.Definition.For.At ?? RuntimeExpressions.Arguments.Index] = index;
        var executor = await this.CreateTaskExecutorAsync(task, this.Task.Definition.Do, contextData, arguments, cancellationToken).ConfigureAwait(false);
        await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles error raised during a iteration's execution
    /// </summary>
    /// <param name="executor">The service used to execute the iteration that has faulted</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnIterationFaultAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        if (this.Collection == null) throw new InvalidOperationException("The executor must be initialized before execution");
        ArgumentNullException.ThrowIfNull(executor);
        var error = executor.Task.Instance.Error ?? throw new NullReferenceException();
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of a iteration's execution
    /// </summary>
    /// <param name="executor">The service used to execute the iteration that has completed</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnIterationCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        if (this.Collection == null) throw new InvalidOperationException("The executor must be initialized before execution");
        ArgumentNullException.ThrowIfNull(executor);
        var last = executor.Task.Instance;
        var output = executor.Task.Output!;
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        var index = int.Parse(last.Reference.OriginalString.Split('/', StringSplitOptions.RemoveEmptyEntries)[^2]) + 1;
        if (index == this.Collection.Count)
        {
            await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
            return;
        }
        switch (executor.Task.Instance.Next)
        {
            case FlowDirective.Continue:
                var next = await this.Task.Workflow.CreateTaskAsync(this.Task.Definition.Do, this.GetPathFor(index.ToString()), output, null, this.Task, false, cancellationToken).ConfigureAwait(false);
                var item = this.Collection.ElementAt(index);
                var contextData = this.Task.ContextData.Clone()!;
                var arguments = this.Task.Arguments.Clone()!;
                arguments[this.Task.Definition.For.Each ?? RuntimeExpressions.Arguments.Each] = item;
                arguments[this.Task.Definition.For.At ?? RuntimeExpressions.Arguments.Index] = index;
                var nextExecutor = await this.CreateTaskExecutorAsync(next, this.Task.Definition.Do, contextData, arguments, cancellationToken).ConfigureAwait(false);
                await nextExecutor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                break;
            case FlowDirective.End:
                await this.SetResultAsync(output, FlowDirective.End, cancellationToken).ConfigureAwait(false);
                break;
            case FlowDirective.Exit:
                await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
                break;
            default:
                await this.SetErrorAsync(Error.Configuration(this.Task.Instance.Reference, "Unable to continue with a specific task within a loop"), cancellationToken).ConfigureAwait(false);
                break;
        }
    }

}
