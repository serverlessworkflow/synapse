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
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="TryTaskDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class TryTaskExecutor(IServiceProvider serviceProvider, ILogger<TryTaskExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<TryTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<TryTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var task = await this.Task.Workflow.CreateTaskAsync(this.Task.Definition.Try, nameof(this.Task.Definition.Try).ToCamelCase(), this.Task.Input, null, this.Task, false, cancellationToken).ConfigureAwait(false);
        var executor = await this.CreateTaskExecutorAsync(task, this.Task.Definition.Try, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync
        (
            _ => System.Threading.Tasks.Task.CompletedTask,
            async (ex) => await this.OnTryFaultedAsync(executor, ex, cancellationToken).ConfigureAwait(false),
            async () => await this.OnTryCompletedAsync(executor, cancellationToken).ConfigureAwait(false),
            cancellationToken
        );
        await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task DoRetryAsync(Error cause, CancellationToken cancellationToken) 
    {
        var task = await this.Task.Workflow.CreateTaskAsync(this.Task.Definition.Try, $"retry/{this.Task.Instance.Retries?.Count - 1}", this.Task.Input, null, this.Task, false, cancellationToken).ConfigureAwait(false);
        var executor = await this.CreateTaskExecutorAsync(task, this.Task.Definition.Try, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        executor.SubscribeAsync
        (
            _ => System.Threading.Tasks.Task.CompletedTask,
            async (ex) => await this.OnTryFaultedAsync(executor, ex, cancellationToken).ConfigureAwait(false),
            async () => await this.OnTryCompletedAsync(executor, cancellationToken).ConfigureAwait(false),
            cancellationToken
        );
        await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles an <see cref="Exception"/> that has occurred during the execution of the task defined by the try statement
    /// </summary>
    /// <param name="executor">The service used to execute the faulted <see cref="TaskInstance"/></param>
    /// <param name="ex">The <see cref="Exception"/> that has occurred</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnTryFaultedAsync(ITaskExecutor executor, Exception ex, CancellationToken cancellationToken)
    {
        var error = ex is ErrorRaisedException errorEx ? errorEx.Error : new()
        {
            Status = ErrorStatus.Runtime,
            Type = ErrorType.Runtime,
            Title = ErrorTitle.Runtime,
            Detail = ex.Message
        };
        var evaluationArguments = this.GetExpressionEvaluationArguments() ?? new Dictionary<string, object>();
        evaluationArguments[this.Task.Definition.Catch.As ?? RuntimeExpressions.Arguments.Error] = error;
        if (this.Task.Definition.Catch.Errors != null && this.Task.Definition.Catch.Errors.With != null && !await this.Task.Workflow.Expressions.MatchesAsync(error, this.Task.Definition.Catch.Errors.With!, evaluationArguments, cancellationToken).ConfigureAwait(false))
        {
            await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (!string.IsNullOrWhiteSpace(this.Task.Definition.Catch.ExceptWhen) && await this.Task.Workflow.Expressions.EvaluateAsync<bool>(this.Task.Definition.Catch.ExceptWhen, error, evaluationArguments, cancellationToken).ConfigureAwait(false))
        {
            await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (!string.IsNullOrWhiteSpace(this.Task.Definition.Catch.When) && !await this.Task.Workflow.Expressions.EvaluateAsync<bool>(this.Task.Definition.Catch.When, error, evaluationArguments, cancellationToken).ConfigureAwait(false))
        {
            await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
            return;
        }
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        if (this.Task.Definition.Catch.Retry != null)
        {
            var limit = this.Task.Definition.Catch.Retry.Limit;
            var limitReached = false;
            if (limit != null)
            {
                if (limit.Attempt != null)
                {
                    if (limit.Attempt.Count.HasValue && this.Task.Instance.Retries?.Count >= limit.Attempt.Count) limitReached = true;
                }
            }
            if (limitReached) await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
            else await this.RetryAsync(error, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (this.Task.Definition.Catch.Do != null)
        {
            var next = await this.Task.Workflow.CreateTaskAsync(this.Task.Definition.Catch.Do, $"{nameof(this.Task.Definition.Catch).ToCamelCase()}/{nameof(ErrorCatcherDefinition.Do).ToCamelCase()}", this.Task.Input, null, this.Task, false, cancellationToken).ConfigureAwait(false);
            var arguments = this.Task.Arguments.Clone()!;
            arguments[this.Task.Definition.Catch.As ?? RuntimeExpressions.Arguments.Error] = error;
            var nextExecutor = await this.CreateTaskExecutorAsync(next, this.Task.Definition.Catch.Do, this.Task.ContextData, arguments, cancellationToken).ConfigureAwait(false);
            nextExecutor.SubscribeAsync
            (
                _ => System.Threading.Tasks.Task.CompletedTask,
                async (ex) => await this.OnHandlerFaultAsync(nextExecutor, ex, cancellationToken).ConfigureAwait(false),
                async () => await this.OnHandlerCompletedAsync(nextExecutor, cancellationToken).ConfigureAwait(false),
                cancellationToken
            );
            await nextExecutor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        await this.SetResultAsync(null, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of the task defined by the try statement
    /// </summary>
    /// <param name="executor">The service used to execute the <see cref="TaskInstance"/> that ran to completion</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnTryCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        var last = executor.Task.Instance;
        var output = executor.Task.Output!;
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        await this.SetResultAsync(output, last.Next == FlowDirective.End ? FlowDirective.End : this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles <see cref="Exception"/>s thrown during the execution of the defined error handler task
    /// </summary>
    /// <param name="executor">The service used to execute the error handler <see cref="TaskInstance"/></param>
    /// <param name="ex">The <see cref="Exception"/> that has been thrown</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnHandlerFaultAsync(ITaskExecutor executor, Exception ex, CancellationToken cancellationToken)
    { 
        ArgumentNullException.ThrowIfNull(executor);
        var error = executor.Task.Instance.Error ?? throw new NullReferenceException();
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        await this.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of the defined error handler task
    /// </summary>
    /// <param name="executor">The service used to execute the error handler <see cref="TaskInstance"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnHandlerCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(executor);
        var last = executor.Task.Instance;
        var output = executor.Task.Output!;
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        await this.SetResultAsync(output, last.Next == FlowDirective.End ? FlowDirective.End : this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}
