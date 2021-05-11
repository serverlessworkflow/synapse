using ConcurrentCollections;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Synapse.Domain.Models;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowActivityProcessor{TActivity}"/> interface
    /// </summary>
    public abstract class WorkflowActivityProcessor
        : IWorkflowActivityProcessor
    {

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        protected WorkflowActivityProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, V1WorkflowActivity activity)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.ExecutionContext = executionContext;
            this.ActivityProcessorFactory = activityProcessorFactory;
            this.Activity = activity;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowExecutionContext"/>
        /// </summary>
        protected IWorkflowExecutionContext ExecutionContext { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected IWorkflowActivityProcessorFactory ActivityProcessorFactory { get; }

        /// <inheritdoc/>
        public V1WorkflowActivity Activity { get; }

        /// <summary>
        /// Gets the <see cref="Subject{T}"/> used to observe the <see cref="WorkflowActivityProcessor"/>'s execution
        /// </summary>
        protected Subject<V1WorkflowExecutionResult> Subject { get; } = new Subject<V1WorkflowExecutionResult>();

        /// <summary>
        /// Gets a <see cref="ConcurrentHashSet{T}"/> containing all child <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected ConcurrentHashSet<IWorkflowActivityProcessor> Processors { get; } = new ConcurrentHashSet<IWorkflowActivityProcessor>();

        /// <summary>
        /// Gets the <see cref="IWorkflowActivityProcessor"/>'s <see cref="System.Threading.CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets the object used to asynchronously lock the <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        protected AsyncLock Lock { get; } = new AsyncLock();

        Task IWorkflowActivityProcessor.ProcessAsync(CancellationToken cancellationToken)
        {
            this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    this.Logger.LogInformation("Initializing activity '{activityId}' (type: '{activityType}')...", this.Activity.Id, this.Activity.Type);
                    await this.ExecutionContext.InitializeActivityAsync(this.Activity, cancellationToken);
                    await this.InitializeAsync(this.CancellationTokenSource.Token);
                    this.Logger.LogInformation("Activity '{activityId}' (type: '{activityType}') initialized", this.Activity.Id, this.Activity.Type);
                    this.Logger.LogInformation("Executing activity '{activityId}' (type: '{activityType}')...", this.Activity.Id, this.Activity.Type);
                    await this.ExecutionContext.ProcessActivityAsync(this.Activity, this.CancellationTokenSource.Token);
                    await this.ProcessAsync(this.CancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    await this.OnErrorAsync(ex, cancellationToken);
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected abstract Task InitializeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected abstract Task ProcessAsync(CancellationToken cancellationToken);

        async Task IWorkflowActivityProcessor.SuspendAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogInformation("Suspending activity '{activityId}' (type: '{activityType}')...", this.Activity.Id, this.Activity.Type);
            await this.ExecutionContext.SuspendActivityAsync(this.Activity, this.CancellationTokenSource.Token);
            await this.SuspendAsync(cancellationToken);
            this.Logger.LogInformation("Activity '{activityId}' (type: '{activityType}') suspended", this.Activity.Id, this.Activity.Type);
        }

        /// <summary>
        /// Suspends the <see cref="V1WorkflowActivity"/>'s processing
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task SuspendAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        async Task IWorkflowActivityProcessor.TerminateAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogInformation("Terminating activity '{activityId}' (type: '{activityType}')...", this.Activity.Id, this.Activity.Type);
            await this.ExecutionContext.TerminateActivityAsync(this.Activity, this.CancellationTokenSource.Token);
            await this.TerminateAsync(cancellationToken);
            this.CancellationTokenSource?.Cancel();
            this.Logger.LogInformation("Activity '{activityId}' (type: '{activityType}') terminated", this.Activity.Id, this.Activity.Type);
        }

        /// <summary>
        /// Terminates the <see cref="IWorkflowActivityProcessor"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task TerminateAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<V1WorkflowExecutionResult> observer)
        {
            return this.Subject.Subscribe(observer);
        }

        /// <summary>
        /// Creates a new child <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a child <see cref="IWorkflowActivityProcessor"/> for</param>
        protected virtual IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            IWorkflowActivityProcessor processor = this.ActivityProcessorFactory.Create(activity);
            this.Processors.Add(processor);
            return processor;
        }

        /// <summary>
        /// Handles the <see cref="V1WorkflowActivity"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnResultAsync(V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            try
            {
                this.Logger.LogInformation("Activity '{activityId}' (type: '{activityType}') executed", this.Activity.Id, this.Activity.Type);
                await this.ExecutionContext.SetActivityResultAsync(this.Activity, result, cancellationToken);
                this.Subject.OnNext(result);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the specified <see cref="Exception"/> that occured during the <see cref="V1WorkflowActivity"/>'s processing
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnErrorAsync(Exception ex, CancellationToken cancellationToken)
        {
            try
            {
                this.Logger.LogWarning($"An error occured while executing the activity '{{activityId}}' (type: '{{activityType}}'){Environment.NewLine}Details: {{ex}}", this.Activity.Id, this.Activity.Type, ex.ToString());
                await this.ExecutionContext.FaultActivityAsync(this.Activity, ex, cancellationToken);
                this.Subject.OnError(ex);
            }
            catch (Exception cex)
            {
                if (cex is HttpOperationException httpEx)
                    this.Logger.LogError($"A critical exception occured while faulting the execution of the activity '{{activityId}}'(type: '{{activityType}}'): the Kubernetes API returned an non-success status code '{{statusCode}}''{Environment.NewLine}Response content: {{responseContent}}{Environment.NewLine}Details: {{ex}}", this.Activity.Id, this.Activity.Type, httpEx.Response.StatusCode, httpEx.Response.Content, ex.ToString());
                else
                    this.Logger.LogError($"A critical exception occured while faulting the execution of the activity '{{activityId}}'(type: '{{activityType}}'):{ Environment.NewLine}{{ex}}", this.Activity.Id, this.Activity.Type, cex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="V1WorkflowActivity"/>'s completion
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnCompletedAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.Processors.ToList().ForEach(p => p.Dispose());
                this.Processors.Clear();
                this.Subject.OnCompleted();
            }
            catch(Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <summary>
        /// Disposes of the <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowActivityProcessor"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    //this.CancellationTokenSource?.Dispose();
                    //this.Processors?.ToList().ForEach(p => p.Dispose());
                    //this.Processors?.Clear();
                    //this.Subject?.Dispose();
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}
