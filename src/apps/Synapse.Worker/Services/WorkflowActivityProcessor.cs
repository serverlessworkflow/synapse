/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using ConcurrentCollections;
using Synapse.Integration.Events;
using Synapse.Integration.Events.WorkflowActivities;
using System.Reactive.Subjects;

namespace Synapse.Worker.Services
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
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        protected WorkflowActivityProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Context = context;
            this.ActivityProcessorFactory = activityProcessorFactory;
            this.Options = options.Value;
            this.Activity = activity;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntimeContext"/>
        /// </summary>
        protected IWorkflowRuntimeContext Context { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected IWorkflowActivityProcessorFactory ActivityProcessorFactory { get; }

        /// <summary>
        /// Gets the current <see cref="ApplicationOptions"/>
        /// </summary>
        protected ApplicationOptions Options { get; }

        /// <inheritdoc/>
        public V1WorkflowActivity Activity { get; }

        /// <summary>
        /// Gets the <see cref="Subject{T}"/> used to observe the <see cref="WorkflowActivityProcessor"/>'s execution
        /// </summary>
        protected Subject<IV1WorkflowActivityIntegrationEvent> Subject { get; } = new();

        /// <summary>
        /// Gets a <see cref="ConcurrentHashSet{T}"/> containing all child <see cref="IWorkflowActivityProcessor"/>s
        /// </summary>
        protected ConcurrentHashSet<IWorkflowActivityProcessor> Processors { get; } = new ConcurrentHashSet<IWorkflowActivityProcessor>();

        /// <summary>
        /// Gets the <see cref="IWorkflowActivityProcessor"/>'s <see cref="System.Threading.CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; private set; } = null!;

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
                    if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
                        await this.Context.Workflow.InitializeActivityAsync(this.Activity, cancellationToken);
                    await this.InitializeAsync(this.CancellationTokenSource.Token);
                    this.Logger.LogInformation("Activity '{activityId}' (type: '{activityType}') initialized", this.Activity.Id, this.Activity.Type);
                    this.Logger.LogInformation("Starting/resuming activity '{activityId}' (type: '{activityType}')...", this.Activity.Id, this.Activity.Type);
                    await this.Context.Workflow.StartOrResumeActivityAsync(this.Activity, this.CancellationTokenSource.Token);
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
            var tasks = new List<Task>(this.Processors.Count);
            foreach (var processor in this.Processors)
            {
                tasks.Add(processor.SuspendAsync(cancellationToken));
            }
            try { await Task.WhenAll(tasks); } catch { }
            await this.Context.Workflow.SuspendActivityAsync(this.Activity, this.CancellationTokenSource.Token);
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
            var tasks = new List<Task>(this.Processors.Count);
            foreach (var processor in this.Processors)
            {
                tasks.Add(processor.TerminateAsync(cancellationToken));
            }
            try { await Task.WhenAll(tasks); } catch { }
            await this.Context.Workflow.CancelActivityAsync(this.Activity, this.CancellationTokenSource.Token);
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
        public IDisposable Subscribe(IObserver<IV1WorkflowActivityIntegrationEvent> observer)
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
        /// Handles the <see cref="V1WorkflowActivity"/>'s <see cref="IV1WorkflowActivityIntegrationEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="IV1WorkflowActivityIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnNextAsync(IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken)
        {
            try
            {
                if(e is V1WorkflowActivityCompletedIntegrationEvent)
                    this.Logger.LogInformation("Activity '{activityId}' (type: '{activityType}') completed", this.Activity.Id, this.Activity.Type);
                await this.Context.Workflow.On(this.Activity, e, cancellationToken);
                this.Subject.OnNext(e);
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
                this.Logger.LogWarning("An error occured while executing the activity '{activityId}' (type: '{activityType}')/r/nDetails:/r/n{ex}", this.Activity.Id, this.Activity.Type, ex.ToString());
                await this.Context.Workflow.FaultActivityAsync(this.Activity, ex, cancellationToken);
                this.Subject.OnError(ex);
            }
            catch (Exception cex)
            {
                this.Logger.LogError("A critical exception occured while faulting the execution of the activity '{activityId}'(type: '{activityType}'):/r/n{ex}", this.Activity.Id, this.Activity.Type, cex.ToString());
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
            catch (Exception ex)
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
                    this.CancellationTokenSource?.Dispose();
                    this.Processors?.ToList().ForEach(p => p.Dispose());
                    this.Processors?.Clear();
                    this.Subject?.Dispose();
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
