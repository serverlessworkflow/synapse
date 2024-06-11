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

using Neuroglia.Data.Expressions;
using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ITaskExecutor"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The <see cref="ITaskExecutionContext"/> in which to run the <see cref="ITaskExecutor"/></param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <typeparam name="TDefinition">The type of <see cref="TaskInstance"/> to run</typeparam>
public abstract class TaskExecutor<TDefinition>(IServiceProvider serviceProvider, ILogger logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<TDefinition> context, IJsonSerializer jsonSerializer)
    : ITaskExecutor<TDefinition>
    where TDefinition : TaskDefinition
{

    bool _disposed;

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to create <see cref="ITaskExecutionContext"/>s
    /// </summary>
    protected ITaskExecutionContextFactory ExecutionContextFactory { get; } = executionContextFactory;

    /// <summary>
    /// Gets the service used to create <see cref="ITaskExecutor"/>s
    /// </summary>
    protected ITaskExecutorFactory ExecutorFactory { get; } = executorFactory;

    /// <summary>
    /// Gets the <see cref="ITaskExecutionContext"/> that describes the execution of the <see cref="TaskInstance"/> to run
    /// </summary>
    public virtual ITaskExecutionContext<TDefinition> Task { get; } = context;

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the <see cref="ISubject{T}"/> used to stream <see cref="ITaskLifeCycleEvent"/>s
    /// </summary>
    protected Subject<ITaskLifeCycleEvent> Subject { get; } = new();

    /// <summary>
    /// Gets a <see cref="ConcurrentHashSet{T}"/> containing all child <see cref="ITaskExecutor"/>s
    /// </summary>
    protected ConcurrentHashSet<ITaskExecutor> Executors { get; } = [];

    /// <summary>
    /// Gets the <see cref="ITaskExecutor"/>'s <see cref="System.Threading.Tasks.TaskCompletionSource"/>
    /// </summary>
    protected TaskCompletionSource TaskCompletionSource { get; } = new();

    /// <summary>
    /// Gets the <see cref="ITaskExecutor"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource? CancellationTokenSource { get; set; }

    /// <summary>
    /// Gets the object used to asynchronously lock the <see cref="TaskExecutor{TDefinition}"/>
    /// </summary>
    protected AsyncLock Lock { get; } = new();

    /// <summary>
    /// Gets the <see cref="TaskExecutor{TDefinition}"/>'s <see cref="System.Diagnostics.Stopwatch"/>, used to clock the <see cref="TaskInstance"/>'s execution
    /// </summary>
    protected Stopwatch Stopwatch { get; } = new();

    /// <summary>
    /// Gets the timer, if any, used to timeout the execution of the <see cref="TaskInstance"/> to run
    /// </summary>
    protected Timer? Timer { get; set; }

    /// <summary>
    /// Gets a key/definition mapping of the extensions, if any, that apply to the task to run
    /// </summary>
    protected IEnumerable<KeyValuePair<string, ExtensionDefinition>>? Extensions => this.Task.Workflow.Definition.Use?.Extensions?.Where(ex => ex.Value.Extend == "all" || ex.Value.Extend == this.Task.Definition.Type);

    ITaskExecutionContext ITaskExecutor.Task => this.Task;

    /// <inheritdoc/>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await this.DoInitializeAsync(cancellationToken).ConfigureAwait(false);
        await this.Task.InitializeAsync(cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new TaskLifeCycleEvent(TaskLifeCycleEventType.Initialized));
    }

    /// <summary>
    /// Initializes the <see cref="TaskInstance"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task DoInitializeAsync(CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (this.Task.Instance.Status != null && !this.Task.Instance.IsOperative) return;
        if (this.Task.Definition.Timeout?.After != null)
        {
            var duration = this.Task.Definition.Timeout.After.ToTimeSpan();
            this.CancellationTokenSource.CancelAfter(duration);
            this.Timer = new(this.OnTimeoutAsync, null, duration, Timeout.InfiniteTimeSpan);
        }
        try
        {
            await this.Task.ExecuteAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
            this.Subject.OnNext(new TaskLifeCycleEvent(TaskLifeCycleEventType.Running));
            this.Stopwatch.Start();
            await this.BeforeExecuteAsync(cancellationToken).ConfigureAwait(false); //todo: act upon last directive
            await this.DoExecuteAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
            await this.TaskCompletionSource.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Executes code before the task executes, typically extensions, if any
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="System.Threading.Tasks.Task"/></returns>
    protected virtual async Task BeforeExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.Task.Instance.IsExtension || this.Extensions == null) return;
        var input = this.Task.Input;
        foreach (var extension in this.Extensions.Where(ex => ex.Value.Before != null).Reverse())
        {
            var taskDefinition = extension.Value.Before!;
            var task = await this.Task.Workflow.CreateTaskAsync(taskDefinition, $"before/{extension.Key}", input, null, this.Task, true, cancellationToken).ConfigureAwait(false);
            var executor = await this.CreateTaskExecutorAsync(task, taskDefinition, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
            await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            if (executor.Task.Instance.Next == FlowDirective.Exit)
            {
                await this.SetResultAsync(executor.Task.Output, executor.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
                return;
            }
            input = executor.Task.Output ?? new();
            this.Executors.Remove(executor);
            await executor.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Runs the <see cref="TaskInstance"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected abstract Task DoExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes code after the task executes, typically extensions, if any
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="System.Threading.Tasks.Task"/></returns>
    protected virtual async Task AfterExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.Task.Instance.IsExtension || this.Extensions == null) return;
        var output = this.Task.Output ?? new { };
        foreach (var extension in this.Extensions.Where(ex => ex.Value.After != null).Reverse())
        {
            var taskDefinition = extension.Value.After!;
            var task = await this.Task.Workflow.CreateTaskAsync(taskDefinition, $"after/{extension.Key}", output, null, this.Task, true, cancellationToken).ConfigureAwait(false);
            var executor = await this.CreateTaskExecutorAsync(task, taskDefinition, this.Task.ContextData, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
            await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            if (executor.Task.Instance.Next == FlowDirective.Exit) break;
            output = executor.Task.Output ?? new();
            this.Executors.Remove(executor);
            await executor.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
    {
        this.Stopwatch.Stop();
        await this.DoSuspendAsync(cancellationToken).ConfigureAwait(false);
        await this.Task.SuspendAsync(cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new TaskLifeCycleEvent(TaskLifeCycleEventType.Suspended));
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetResult();
    }

    /// <summary>
    /// Suspends the <see cref="TaskInstance"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task DoSuspendAsync(CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task RetryAsync(Error cause, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cause);
        this.Stopwatch.Stop();
        await this.Task.RetryAsync(cause, cancellationToken).ConfigureAwait(false);
        await this.DoRetryAsync(cause, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retries to run the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="cause">The <see cref="Error"/> that caused the retry attempt</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task DoRetryAsync(Error cause, CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task SetErrorAsync(Error error, CancellationToken cancellationToken = default)
    {
        this.Stopwatch.Stop();
        await this.DoSetErrorAsync(error, cancellationToken).ConfigureAwait(false);
        await this.Task.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new TaskLifeCycleEvent(TaskLifeCycleEventType.Faulted));
        this.Subject.OnError(new ErrorRaisedException(error));
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetResult();
    }

    /// <summary>
    /// Faults the handled <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="error"><see cref="Error"/> to set</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task DoSetErrorAsync(Error error, CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task SetResultAsync(object? result, string? then = FlowDirective.Continue, CancellationToken cancellationToken = default)
    {
        if (this.Task.Instance.Status != TaskInstanceStatus.Running) return;
        this.Stopwatch.Stop();
        if (string.IsNullOrWhiteSpace(then)) then = FlowDirective.Continue;
        var output = result;
        var arguments = this.GetExpressionEvaluationArguments() ?? new Dictionary<string, object>();
        arguments["output"] = output!;//todo: replace with arguments[RuntimeExpressions.Arguments.Output] = output;
        if (this.Task.Definition.Output?.From is string fromExpression) output = await this.Task.Workflow.Expressions.EvaluateAsync<object>(fromExpression, output ?? new(), arguments, cancellationToken).ConfigureAwait(false);
        else if (this.Task.Definition.Output?.From != null) output = await this.Task.Workflow.Expressions.EvaluateAsync<object>(this.Task.Definition.Output.From, output ?? new(), arguments, cancellationToken).ConfigureAwait(false);
        if (this.Task.Definition.Output?.To is string toExpression) 
        {
            var context = (await this.Task.Workflow.Expressions.EvaluateAsync<IDictionary<string, object>>(toExpression, this.Task.ContextData, arguments, cancellationToken).ConfigureAwait(false))!;
            await this.Task.SetContextDataAsync(context, cancellationToken).ConfigureAwait(false);
        }
        else if (this.Task.Definition.Output?.To != null)
        {
            var context = (await this.Task.Workflow.Expressions.EvaluateAsync<IDictionary<string, object>>(this.Task.Definition.Output.To, this.Task.ContextData, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false))!;
            await this.Task.SetContextDataAsync(context, cancellationToken).ConfigureAwait(false);
        }
        await this.AfterExecuteAsync(cancellationToken).ConfigureAwait(false); //todo: act upon last directive
        await this.DoSetResultAsync(output, then, cancellationToken).ConfigureAwait(false);
        await this.Task.SetResultAsync(output, then, cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new TaskLifeCycleEvent(TaskLifeCycleEventType.Completed));
        this.Subject.OnCompleted();
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetResult();
    }

    /// <summary>
    /// Sets the <see cref="TaskInstance"/>'s result and transitions to '<see cref="TaskInstanceStatus.Completed"/>'.
    /// </summary>
    /// <param name="result">The <see cref="TaskInstance"/>'s result, if any</param>
    /// <param name="then">The <see cref="FlowDirective"/> to perform next</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task DoSetResultAsync(object? result, string then, CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;

    /// <inheritdoc/>
    public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await this.Task.CancelAsync(cancellationToken).ConfigureAwait(false);
        await this.DoCancelAsync(cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new TaskLifeCycleEvent(TaskLifeCycleEventType.Cancelled));
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetCanceled(cancellationToken);
    }

    /// <summary>
    /// Cancels the <see cref="TaskInstance"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task DoCancelAsync(CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;

    /// <summary>
    /// Creates a new <see cref="ITaskExecutor"/> for the specified <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="task">The <see cref="TaskInstance"/> to create a new <see cref="ITaskExecutor"/> for</param>
    /// <param name="definition">The <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to execute</param>
    /// <param name="contextData">The current context data</param>
    /// <param name="arguments">A name/value mapping of the task's arguments, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="ITaskExecutor"/></returns>
    protected virtual async Task<ITaskExecutor> CreateTaskExecutorAsync(TaskInstance task, TaskDefinition definition, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(contextData);
        var input = (await this.Task.Workflow.Documents.GetAsync(task.InputReference!, cancellationToken).ConfigureAwait(false)).Content;
        var context = this.ExecutionContextFactory.Create(this.Task.Workflow, task, definition, input, contextData, arguments);
        var executor = this.ExecutorFactory.Create(this.ServiceProvider, context);
        await executor.InitializeAsync(cancellationToken).ConfigureAwait(false);
        this.Executors.Add(executor);
        return executor;
    }

    /// <summary>
    /// Gets a new <see cref="IDictionary{TKey, TValue}"/>, if any, containing the runtime expression evaluation arguments for the <see cref="TaskInstance"/> to run
    /// </summary>
    /// <returns>A new <see cref="IDictionary{TKey, TValue}"/>, if any, containing the runtime expression evaluation arguments for the <see cref="TaskInstance"/> to run</returns>
    protected virtual IDictionary<string, object>? GetExpressionEvaluationArguments()
    {
        var parameters = this.Task.Arguments.Clone()!;
        parameters[RuntimeExpressions.Arguments.Context] = this.Task.ContextData;
        parameters[RuntimeExpressions.Arguments.Workflow] = this.Task.Workflow.Instance;
        parameters[RuntimeExpressions.Arguments.Task] = this.Task.Instance;
        parameters[RuntimeExpressions.Arguments.Input] = this.Task.Input;
        return parameters;
    }

    /// <inheritdoc/>
    public virtual IDisposable Subscribe(IObserver<ITaskLifeCycleEvent> observer) => this.Subject.Subscribe(observer);

    /// <summary>
    /// Handles the timeout of the <see cref="TaskInstance"/> to execute
    /// </summary>
    /// <param name="state">The timer's state</param>
    protected virtual async void OnTimeoutAsync(object? state)
    {
        await this.SetErrorAsync(new Error()
        {
            Status = (int)HttpStatusCode.RequestTimeout,
            Type = ErrorType.Timeout,
            Title = ErrorTitle.Timeout,
            Detail = $"The task with name '{this.Task.Instance.Name}' at '{this.Task.Instance.Reference}' has timed out after {this.Task.Definition.Timeout!.After.Milliseconds} milliseconds"
        }, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="TaskExecutor{TDefinition}"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="TaskExecutor{TDefinition}"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (this._disposed) return;
        foreach (var executor in this.Executors)
        {
            try { await executor.DisposeAsync().ConfigureAwait(false); }
            catch { }
        }
        this.Subject.Dispose();
        this.Executors.Clear();
        this.CancellationTokenSource?.Dispose();
        if (this.Timer != null) await this.Timer.DisposeAsync().ConfigureAwait(false);
        this._disposed = true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="TaskExecutor{TDefinition}"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="TaskExecutor{TDefinition}"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;
        foreach (var executor in this.Executors)
        {
            try { executor.Dispose(); }
            catch { }
        }
        this.Subject.Dispose();
        this.Executors.Clear();
        this.CancellationTokenSource?.Dispose();
        this.Timer?.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}