using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{
    /// <summary>
    /// Represents an <see cref="ActionProcessor"/> used to process <see cref="EventReference"/>s
    /// </summary>
    public class AsyncFunctionProcessor
        : ActionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="AsyncFunctionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="EventReference"/> to process belongs to</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="triggerEvent">The <see cref="EventDefinition"/> that defines the <see cref="CloudEvent"/> to produce, thus triggering the async call</param>
        /// <param name="resultEvent">The <see cref="EventDefinition"/> that defines the <see cref="CloudEvent"/> to consume, thus ending the async call</param>
        public AsyncFunctionProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, 
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, StateDefinition state, ActionDefinition action, EventDefinition triggerEvent, EventDefinition resultEvent)
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity, action)
        {
            this.State = state;
            this.TriggerEvent = triggerEvent;
            this.ResultEvent = resultEvent;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> the <see cref="EventReference"/> to process belongs to
        /// </summary>
        protected StateDefinition State { get; }

        /// <summary>
        /// Gets the <see cref="EventDefinition"/> that defines the <see cref="CloudEvent"/> to produce, thus triggering the async call
        /// </summary>
        protected EventDefinition TriggerEvent { get; }

        /// <summary>
        /// Gets the <see cref="EventDefinition"/> that defines the <see cref="CloudEvent"/> to consume, thus ending the async call
        /// </summary>
        protected EventDefinition ResultEvent { get; }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            IWorkflowActivityProcessor processor = base.CreateProcessorFor(activity);
            CancellationToken cancellationToken = this.CancellationTokenSource.Token;
            switch (processor)
            {
                case ProduceEventProcessor produceEventProcessor:
                    processor.SubscribeAsync
                    (
                        async result => await this.OnProduceEventResultAsync(result, cancellationToken),
                        async ex => await this.OnErrorAsync(ex, cancellationToken),
                        async () => await this.OnProduceEventCompletedAsync(processor, cancellationToken)
                    );
                    break;
                case ConsumeEventProcessor consumeEventProcessor:
                    processor.SubscribeAsync
                    (
                        async result => await this.OnConsumeEventResultAsync(result, cancellationToken),
                        async ex => await this.OnErrorAsync(ex, cancellationToken),
                        async () => await this.OnConsumeEventCompletedAsync(processor, cancellationToken)
                    );
                    break;
                default:
                    processor.Dispose();
                    throw new NotSupportedException($"The specified {nameof(IWorkflowActivityProcessor)} '{processor.GetType()}' is not supported");
            }
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status <= V1WorkflowActivityStatus.Initializing)
                await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.ProduceEvent(this.ExecutionContext.Instance, this.State, this.TriggerEvent.Name, this.ExecutionContext.ExpressionEvaluator.FilterInput(this.Action, this.Activity.Data), this.Activity), cancellationToken);
            foreach (V1WorkflowActivity activity in await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
            {
                this.CreateProcessorFor(activity);
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (IWorkflowActivityProcessor processor in this.Processors.ToList())
            {
                await processor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowExecutionResult"/> of a <see cref="ProduceEventProcessor"/>
        /// </summary>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task OnProduceEventResultAsync(V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the completion of a <see cref="ProduceEventProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has run to completion</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnProduceEventCompletedAsync(IWorkflowActivityProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            V1WorkflowActivity childPointer = await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.ConsumeEvent(this.ExecutionContext.Instance, this.State, this.ResultEvent.Name, this.ExecutionContext.ExpressionEvaluator.FilterInput(this.Action, this.Activity.Data), this.Activity), cancellationToken);
            IWorkflowActivityProcessor childProcessor = this.CreateProcessorFor(childPointer);
            await childProcessor.ProcessAsync(cancellationToken);
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowExecutionResult"/> of a <see cref="ConsumeEventProcessor"/>
        /// </summary>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnConsumeEventResultAsync(V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            if (result.Type == V1WorkflowExecutionResultType.Next)
                await this.OnResultAsync(V1WorkflowExecutionResult.Next(this.ExecutionContext.ExpressionEvaluator.FilterResults(this.Action, result.Output)), cancellationToken);
        }

        /// <summary>
        /// Handles the completion of a <see cref="ConsumeEventProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that has run to completion</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnConsumeEventCompletedAsync(IWorkflowActivityProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            await this.OnCompletedAsync(cancellationToken);
        }

    }

}
