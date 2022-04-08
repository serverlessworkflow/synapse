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

using Synapse.Integration.Events.WorkflowActivities;
using System.Reactive.Linq;

namespace Synapse.Worker.Executor.Services.Processors
{
    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventStateTriggerDefinition"/>s
    /// </summary>
    public class EventStateTriggerProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="jsonSerializer">The service used to serialize/deserialize to/from JSON</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="EventStateDefinition"/> to process</param>
        /// <param name="trigger">The <see cref="EventStateTriggerDefinition"/> to process</param>
        public EventStateTriggerProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, IJsonSerializer jsonSerializer, 
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, EventStateDefinition state, EventStateTriggerDefinition trigger) 
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.JsonSerializer = jsonSerializer;
            this.State = state;
            this.Trigger = trigger;
        }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the <see cref="EventStateDefinition"/> to process
        /// </summary>
        public EventStateDefinition State { get; }

        /// <summary>
        /// Gets the <see cref="EventStateTriggerDefinition"/> to process
        /// </summary>
        public EventStateTriggerDefinition Trigger { get; }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            IWorkflowActivityProcessor processor = base.CreateProcessorFor(activity);
            CancellationToken cancellationToken = this.CancellationTokenSource.Token;
            switch (processor)
            {
                case ConsumeEventProcessor consumeEventProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async result => await this.OnEventResultAsync(consumeEventProcessor, result, cancellationToken),
                        async ex => await this.OnErrorAsync(ex, cancellationToken),
                        async () => await this.OnEventCompletedAsync(consumeEventProcessor, cancellationToken)
                    );
                    break;
                case ActionProcessor actionProcessor:
                    processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
                    (
                        async result => await this.OnActionResultAsync(actionProcessor, result, cancellationToken),
                        async ex => await this.OnErrorAsync(ex, cancellationToken),
                        async () => await this.OnActionCompletedAsync(actionProcessor, cancellationToken)
                    );
                    break;
                default:
                    throw new NotSupportedException($"The specified execution pointer type '{processor.GetType().Name}' is not supported in this context");
            }
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status <= V1WorkflowActivityStatus.Pending)
            {
                foreach (var eventReference in this.Trigger.Events)
                {
                    var metadata = new Dictionary<string, string>()
                    {
                        { V1WorkflowActivityMetadata.State, this.State.Name! },
                        { V1WorkflowActivityMetadata.Event, eventReference }
                    };
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.ConsumeEvent, this.Activity.Input, metadata, this.Activity, cancellationToken);
                }
            }
            foreach (V1WorkflowActivity childActivity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
            {
                this.CreateProcessorFor(childActivity);
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors)
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="ConsumeEventProcessor"/>'s <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="ConsumeEventProcessor"/> that returned the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEventResultAsync(ConsumeEventProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            var consumeEventActivities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                .Where(a => a.Type == V1WorkflowActivityType.ConsumeEvent)
                .ToList();
            if (this.State.Exclusive)
            {
                foreach (IWorkflowActivityProcessor childProcessor in this.Processors
                    .Where(p => p.Activity.Type == V1WorkflowActivityType.ConsumeEvent)
                    .ToList())
                {
                    if (childProcessor == processor)
                        continue;
                    await childProcessor.TerminateAsync(cancellationToken);
                    this.Processors.TryRemove(childProcessor);
                    childProcessor.Dispose();
                }
            }
            if (this.State.Exclusive
                || consumeEventActivities.All(p => p.Status == V1WorkflowActivityStatus.Completed))
            {
                var input = this.Activity.Input.ToObject();
                foreach (var consumeEventActivity in consumeEventActivities
                    .Where(p => p.Status == V1WorkflowActivityStatus.Completed && p.Output != null))
                {
                    input = input.Merge(consumeEventActivity.Output.ToObject());
                }
                var metadata = new Dictionary<string, string>()
                {
                    { V1WorkflowActivityMetadata.State, this.State.Name! },
                    { V1WorkflowActivityMetadata.Trigger, this.State.Triggers.IndexOf(this.Trigger).ToString() }
                };
                switch (this.Trigger.ActionMode)
                {
                    case ActionExecutionMode.Parallel:
                        foreach (ActionDefinition triggerAction in this.Trigger.Actions)
                        {
                            metadata[V1WorkflowActivityMetadata.Action] = triggerAction.Name!;
                            await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, await this.Context.FilterInputAsync(triggerAction, input!), metadata, this.Activity, cancellationToken);
                        }
                        break;
                    case ActionExecutionMode.Sequential:
                        var action = this.Trigger.Actions.First();
                        metadata[V1WorkflowActivityMetadata.Action] = action.Name!;
                        await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, await this.Context.FilterInputAsync(action, input!), metadata, this.Activity, cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.Trigger.ActionMode}' is not supported");
                }
                foreach (V1WorkflowActivity activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="ConsumeEventProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="ConsumeEventProcessor"/> to handle the completion of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEventCompletedAsync(ConsumeEventProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors.ToList())
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="V1WorkflowActivity"/>'s <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has returned the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionResultAsync(ActionProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            if (this.Trigger.ActionMode == ActionExecutionMode.Sequential)
            {
                if (this.Trigger.TryGetNextAction(processor.Action.Name!, out ActionDefinition action))
                {
                    var metadata = new Dictionary<string, string>()
                    {
                        { V1WorkflowActivityMetadata.State, this.State.Name! },
                        { V1WorkflowActivityMetadata.Trigger, this.State.Triggers.IndexOf(this.Trigger).ToString() },
                        { V1WorkflowActivityMetadata.Action, action.Name! }
                    };
                    var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, this.Context.FilterInputAsync(action, e.Output), metadata, this.Activity, cancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    await this.OnNextAsync(e, cancellationToken);
                }
            }
            else
            {
                var childActivities = (await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
                    .Where(p => p.Type == V1WorkflowActivityType.Action
                        && p.Status == V1WorkflowActivityStatus.Completed
                        && p.Output != null)
                    .ToList();
                if (childActivities.Count == this.Trigger.Actions.Count)
                {
                    var output = new object();
                    foreach (var activity in childActivities
                        .Where(p => p.Status == V1WorkflowActivityStatus.Completed && p.Output != null))
                    {
                        if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Trigger, out var rawTriggerId))
                            throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.Trigger}'");
                        if(!int.TryParse(rawTriggerId, out var triggerId))
                            throw new ArgumentException($"The '{V1WorkflowActivityMetadata.Trigger}' metadata field of activity '{activity.Id}' is not a valid integer");
                        if(!this.State.TryGetTrigger(triggerId, out var trigger))
                            throw new NullReferenceException($"Failed to find a trigger ath the specified index '{triggerId}' in the event state with name '{this.State.Name}'");
                        if (!trigger.TryGetAction(rawTriggerId, out var action))
                            throw new NullReferenceException($"Failed to find an action with name '{rawTriggerId}' in the event trigger with at index '{triggerId}', inside state with name '{this.State.Name}'");
                        if (action.UseResults())
                        {
                            var expression = action.ActionDataFilter?.ToStateData?.Trim();
                            var json = await this.JsonSerializer.SerializeAsync(activity.Output, cancellationToken);
                            if (string.IsNullOrWhiteSpace(expression))
                            {
                                expression = json;
                            }
                            else
                            {
                                if (expression.StartsWith("${"))
                                    expression = expression[2..^1];
                                expression = $"{expression} = {json}";
                            }
                            output = await this.Context.EvaluateAsync(expression, output, cancellationToken);
                        }
                    }
                    await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionCompletedAsync(ActionProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            IEnumerable<V1WorkflowActivity> childActivities = await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken);
            if (childActivities
                .Where(a => a.Type != V1WorkflowActivityType.End && a.Type != V1WorkflowActivityType.Transition)
                .All(p => p.Status == V1WorkflowActivityStatus.Completed))
            {
                await base.OnCompletedAsync(cancellationToken);
                return;
            }
            foreach (V1WorkflowActivity activity in childActivities
                .Where(p => p.Type == V1WorkflowActivityType.Action && p.Status == V1WorkflowActivityStatus.Pending))
            {
                var nextProcessor = this.CreateProcessorFor(activity);
                await nextProcessor.ProcessAsync(cancellationToken);
            }
        }

    }

}
