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

using CloudNative.CloudEvents;
using Synapse.Integration.Events;
using Synapse.Integration.Events.WorkflowActivities;
using System.Reactive.Linq;

namespace Synapse.Worker.Services.Processors
{
    /// <summary>
    /// Represents an <see cref="IWorkflowActivityProcessor"/> implementation used to process <see cref="EventReference"/>
    /// </summary>
    public class AsyncFunctionProcessor
        : ActionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="AsyncFunctionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="EventReference"/> to process belongs to</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="triggerEvent">The <see cref="EventDefinition"/> that defines the <see cref="CloudEvent"/> to produce, thus triggering the async call</param>
        /// <param name="resultEvent">The <see cref="EventDefinition"/> that defines the <see cref="CloudEvent"/> to consume, thus ending the async call</param>
        public AsyncFunctionProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, StateDefinition state, ActionDefinition action, EventDefinition triggerEvent, EventDefinition resultEvent)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, action)
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
            var processor = base.CreateProcessorFor(activity);
            var cancellationToken = this.CancellationTokenSource.Token;
            switch (processor)
            {
                case ProduceEventProcessor produceEventProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>()
                        .SubscribeAsync
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
            if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
            {
                var input = await this.Context.FilterInputAsync(this.Action, this.Activity.Input.ToObject()!, cancellationToken);
                var metadata = new Dictionary<string, string>() 
                {
                    {  V1WorkflowActivityMetadata.State, this.State.Name },
                    {  V1WorkflowActivityMetadata.Action, this.Action.Name! },
                    {  V1WorkflowActivityMetadata.Event, this.Action.Event!.ProduceEvent }
                };
                await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.ProduceEvent, input, metadata, this.Activity, cancellationToken);
            }
            foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
        /// Handles the next <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> of a <see cref="ProduceEventProcessor"/>
        /// </summary>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnProduceEventResultAsync(V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            var input = await this.Context.FilterInputAsync(this.Action, this.Activity.Input.ToObject()!, cancellationToken);
            var metadata = new Dictionary<string, string>()
            {
                { V1WorkflowActivityMetadata.State, this.State.Name! },
                { V1WorkflowActivityMetadata.Action, this.Action.Name! },
                { V1WorkflowActivityMetadata.Event, this.Action.Event!.ResultEvent }
            };
            var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.ConsumeEvent, input, metadata, this.Activity, cancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
            foreach(var childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="IV1WorkflowActivityIntegrationEvent"/> of a <see cref="ConsumeEventProcessor"/>
        /// </summary>
        /// <param name="e">The <see cref="IV1WorkflowActivityIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnConsumeEventResultAsync(IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken)
        {
            if (e is V1WorkflowActivityCompletedIntegrationEvent completedEvent)
            {
                var output = await this.Context.FilterOutputAsync(this.Action, completedEvent.Output, cancellationToken);
                if (output == null)
                    output = new();
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
            }
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
