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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents the service used to execute workflows
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="context">The current <see cref="IWorkflowExecutionContext"/></param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
public class WorkflowExecutor(IServiceProvider serviceProvider, ILogger<WorkflowExecutor> logger, IWorkflowExecutionContext context, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, IJsonSerializer jsonSerializer)
    : IWorkflowExecutor
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
    /// Gets the current <see cref="IWorkflowExecutionContext"/>
    /// </summary>
    public IWorkflowExecutionContext Workflow { get; } = context;

    /// <summary>
    /// Gets the service used to create <see cref="ITaskExecutionContext"/>s
    /// </summary>
    protected ITaskExecutionContextFactory ExecutionContextFactory { get; } = executionContextFactory;

    /// <summary>
    /// Gets the service used to create <see cref="ITaskExecutor"/>s
    /// </summary>
    protected ITaskExecutorFactory ExecutorFactory { get; } = executorFactory;

    /// <summary>
    /// Gets the service used to serialize/deserialize objects to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the <see cref="ISubject{T}"/> used to stream <see cref="IWorkflowLifeCycleEvent"/>s
    /// </summary>
    protected Subject<IWorkflowLifeCycleEvent> Subject { get; } = new();

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

    /// <inheritdoc/>
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            switch (this.Workflow.Instance.Status?.Phase)
            {
                case null or WorkflowInstanceStatusPhase.Pending:
                    await this.StartAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
                    break;
                case WorkflowInstanceStatusPhase.Running:
                case WorkflowInstanceStatusPhase.Waiting:
                    await this.ResumeAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
                    break;
                default:
                    this.Logger.LogWarning("The workflow instance '{instance}' is in an unexpected status phase '{statusPhase}'", this.Workflow.Instance.GetQualifiedName(), this.Workflow.Instance.Status?.Phase);
                    return;
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError("A critical exception occurred while executing the workflow instance '{instance}': {ex}", this.Workflow.Instance.GetQualifiedName(), ex);
            await this.Workflow.SetErrorAsync(Error.Runtime(new Uri("/", UriKind.Relative), $"A critical exception occurred while executing the workflow instance '{this.Workflow.Instance.GetQualifiedName()}': {ex}"), this.CancellationTokenSource.Token).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Starts the execution of the workflow
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.Workflow.StartAsync(cancellationToken).ConfigureAwait(false);
        var taskDefinition = this.Workflow.Definition.Do.First(); //todo: we might add much more complex rules here (event based, etc)
        var task = await this.Workflow.CreateTaskAsync(taskDefinition.Value, taskDefinition.Key, this.Workflow.Instance.Spec.Input ?? [], cancellationToken: cancellationToken).ConfigureAwait(false);
        var executor = await this.CreateTaskExecutorAsync(task, taskDefinition.Value, this.Workflow.ContextData, this.Workflow.Arguments, cancellationToken).ConfigureAwait(false);
        await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resumes the execution of the workflow
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ResumeAsync(CancellationToken cancellationToken)
    {
        //todo
        await this.Workflow.ResumeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
    {
        this.Stopwatch.Stop();
        await this.Workflow.SuspendAsync(cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new WorkflowLifeCycleEvent(WorkflowLifeCycleEventType.Suspended));
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetResult();
    }

    /// <summary>
    /// Sets the uncaught error that has occurred during the workflow's execution
    /// </summary>
    /// <param name="error">The uncaught error</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task SetErrorAsync(Error error, CancellationToken cancellationToken = default)
    {
        this.Stopwatch.Stop();
        await this.Workflow.SetErrorAsync(error, cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new WorkflowLifeCycleEvent(WorkflowLifeCycleEventType.Faulted));
        this.Subject.OnError(new ErrorRaisedException(error));
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetResult();
    }

    /// <summary>
    /// Sets the workflow's execution result, if any
    /// </summary>
    /// <param name="result">The workflow's result</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task SetResultAsync(object? result, CancellationToken cancellationToken = default)
    {
        if (this.Workflow.Instance.Status?.Phase != WorkflowInstanceStatusPhase.Running) return;
        this.Stopwatch.Stop();
        var output = result;
        await this.Workflow.SetResultAsync(output, cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new WorkflowLifeCycleEvent(WorkflowLifeCycleEventType.Completed));
        this.Subject.OnCompleted();
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetResult();
    }

    /// <inheritdoc/>
    public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await this.Workflow.CancelAsync(cancellationToken).ConfigureAwait(false);
        this.Subject.OnNext(new WorkflowLifeCycleEvent(WorkflowLifeCycleEventType.Cancelled));
        if (!this.TaskCompletionSource.Task.IsCompleted) this.TaskCompletionSource.SetCanceled(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual IDisposable Subscribe(IObserver<IWorkflowLifeCycleEvent> observer) => this.Subject.Subscribe(observer);

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
        var input = (await this.Workflow.Documents.GetAsync(task.InputReference!, cancellationToken).ConfigureAwait(false)).Content;
        var context = this.ExecutionContextFactory.Create(this.Workflow, task, definition, input, contextData, arguments);
        var executor = this.ExecutorFactory.Create(this.ServiceProvider, context);
        executor.SubscribeAsync
        (
            _ => Task.CompletedTask,
            async ex => await this.OnTaskFaultedAsync(executor, cancellationToken).ConfigureAwait(false),
            async () => await this.OnTaskCompletedAsync(executor, cancellationToken).ConfigureAwait(false)
        );
        await executor.InitializeAsync(cancellationToken).ConfigureAwait(false);
        this.Executors.Add(executor);
        return executor;
    }

    /// <summary>
    /// Handles the timeout of the <see cref="WorkflowInstance"/> to execute
    /// </summary>
    /// <param name="state">The timer's state</param>
    protected virtual async void OnTimeoutAsync(object? state)
    {
        await this.SetErrorAsync(new Error()
        {
            Status = (int)HttpStatusCode.RequestTimeout,
            Type = ErrorType.Timeout,
            Title = ErrorTitle.Timeout,
            Detail = $"The workflow '{this.Workflow.Instance.GetQualifiedName()}' has timed out after {this.Workflow.Definition.Timeout!.After.Milliseconds} milliseconds"
        }, this.CancellationTokenSource?.Token ?? default).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of a top level task
    /// </summary>
    /// <param name="executor">The service used to execute the task</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnTaskFaultedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        await this.SetErrorAsync(executor.Task.Instance.Error ?? throw new Exception("Faulted tasks must document an error"), cancellationToken).ConfigureAwait(false);
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the completion of a top level task
    /// </summary>
    /// <param name="executor">The service used to execute the task</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnTaskCompletedAsync(ITaskExecutor executor, CancellationToken cancellationToken)
    {
        var nextDefinition = executor.Task.Instance.Next switch
        {
            FlowDirective.End or FlowDirective.Exit => default,
            _ => this.Workflow.Definition.GetTaskAfter(executor.Task.Instance)
        };
        var last = executor.Task;
        this.Executors.Remove(executor);
        await executor.DisposeAsync().ConfigureAwait(false);
        if (nextDefinition.Key == null)
        {
            await this.SetResultAsync(last.Output, cancellationToken).ConfigureAwait(false);
            return;
        }
        var next = await this.Workflow.CreateTaskAsync(last.Definition, nextDefinition.Key, last.Output ?? new { }, cancellationToken: cancellationToken).ConfigureAwait(false);
        var nextExecutor = await this.CreateTaskExecutorAsync(next, nextDefinition.Value, this.Workflow.ContextData, this.Workflow.Arguments, cancellationToken).ConfigureAwait(false);
        await nextExecutor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="TaskExecutor{TDefinition}"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="TaskExecutor{TDefinition}"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (this._disposed || !disposing) return;
        foreach (var executor in this.Executors) try { await executor.DisposeAsync().ConfigureAwait(false); } catch { }
        this.Executors.Clear();
        try { this.TaskCompletionSource.SetCanceled(); } catch { }
        this.CancellationTokenSource?.Dispose();
        if (this.Timer != null) await this.Timer.DisposeAsync().ConfigureAwait(false);
        this.Subject.Dispose();
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
        if (this._disposed || !disposing) return;
        foreach (var executor in this.Executors) try { executor.Dispose(); } catch { }
        this.Executors.Clear();
        try { this.TaskCompletionSource.SetCanceled(); } catch { }
        this.CancellationTokenSource?.Dispose();
        this.Timer?.Dispose();
        this.Subject.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}