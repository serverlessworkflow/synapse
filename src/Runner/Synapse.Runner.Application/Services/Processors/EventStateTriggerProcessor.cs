using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventStateTriggerDefinition"/>s
    /// </summary>
    public class EventStateTriggerProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="EventStateTriggerProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="EventStateDefinition"/> to process</param>
        /// <param name="trigger">The <see cref="EventStateTriggerDefinition"/> to process</param>
        public EventStateTriggerProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, EventStateDefinition state, EventStateTriggerDefinition trigger)
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity)
        {
            this.State = state;
            this.Trigger = trigger;
        }

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
                    processor.SubscribeAsync
                    (
                        async result => await this.OnEventResultAsync(consumeEventProcessor, result, cancellationToken),
                        async ex => await this.OnErrorAsync(ex, cancellationToken),
                        async () => await this.OnEventCompletedAsync(consumeEventProcessor, cancellationToken)
                    );
                    break;
                case ActionProcessor actionProcessor:
                    processor.SubscribeAsync
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
            if (this.Activity.Status <= V1WorkflowActivityStatus.Initializing)
            {
                foreach (string eventReference in this.Trigger.Events)
                {
                    await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.ConsumeEvent(this.ExecutionContext.Instance, this.State, eventReference, this.Activity.Data, this.Activity), cancellationToken);
                }
            }
            foreach (V1WorkflowActivity childActivity in await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
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
        /// Handles the next <see cref="ConsumeEventProcessor"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="processor">The <see cref="ConsumeEventProcessor"/> that returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The <see cref="V1WorkflowExecutionResult"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEventResultAsync(ConsumeEventProcessor processor, V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            List<V1WorkflowActivity> consumeEventActivities = (await this.ExecutionContext.ListChildActivitiesAsync(this.Activity, true, cancellationToken))
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
                || consumeEventActivities.All(p => p.Status == V1WorkflowActivityStatus.Executed && p.Result != null))
            {
                JObject input = new();
                foreach (V1WorkflowActivity consumeEventActivity in consumeEventActivities
                    .Where(p => p.Result.Type == V1WorkflowExecutionResultType.Next && p.Result.Output != null))
                {
                    input.Merge(consumeEventActivity.Result.Output);
                }
                switch (this.Trigger.ActionMode)
                {
                    case ActionExecutionMode.Parallel:
                        foreach (ActionDefinition triggerAction in this.Trigger.Actions)
                        {
                            await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Action(this.ExecutionContext.Instance, this.State, this.Trigger, triggerAction, this.ExecutionContext.ExpressionEvaluator.FilterInput(triggerAction, input), this.Activity), cancellationToken);
                        }
                        break;
                    case ActionExecutionMode.Sequential:
                        ActionDefinition action = this.Trigger.Actions.First();
                        await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Action(this.ExecutionContext.Instance, this.State, this.Trigger, action, this.ExecutionContext.ExpressionEvaluator.FilterInput(action, input), this.Activity), cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.Trigger.ActionMode}' is not supported");
                }
                foreach (V1WorkflowActivity activity in await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
                {
                    this.CreateProcessorFor(activity);
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
        /// Handles the next <see cref="V1WorkflowActivity"/>'s <see cref="V1WorkflowExecutionResult"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="result">The returned <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionResultAsync(ActionProcessor processor, V1WorkflowExecutionResult result, CancellationToken cancellationToken)
        {
            if (this.Trigger.ActionMode == ActionExecutionMode.Sequential)
            {
                if (this.Trigger.TryGetNextAction(processor.Action.Name, out ActionDefinition action))
                {
                    V1WorkflowActivity activity = await this.ExecutionContext.CreateActivityAsync(V1WorkflowActivity.Action(this.ExecutionContext.Instance, this.State, this.Trigger, action, this.ExecutionContext.ExpressionEvaluator.FilterInput(action, result.Output), this.Activity), cancellationToken);
                    this.CreateProcessorFor(activity);
                }
                else
                {
                    await this.OnResultAsync(V1WorkflowExecutionResult.Next(result.Output), cancellationToken);
                }
            }
            else
            {
                List<V1WorkflowActivity> childActivities = (await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken))
                    .Where(p => p.Type == V1WorkflowActivityType.Action
                        && p.Status == V1WorkflowActivityStatus.Executed
                        && p.Result.Type == V1WorkflowExecutionResultType.Next
                        && p.Result.Output != null)
                    .ToList();
                if (childActivities.Count == this.Trigger.Actions.Count)
                {
                    JObject output = new();
                    foreach (V1WorkflowActivity childActivity in childActivities
                        .Where(p => p.Result.Type == V1WorkflowExecutionResultType.Next && p.Result.Output != null))
                    {
                        output.Merge(childActivity.Result.Output);
                    }
                    await this.OnResultAsync(new V1WorkflowExecutionResult(V1WorkflowExecutionResultType.Next, output), cancellationToken);
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
            IEnumerable<V1WorkflowActivity> childActivities = await this.ExecutionContext.ListActiveChildActivitiesAsync(this.Activity, cancellationToken);
            if (childActivities
                .Where(a => a.Type != V1WorkflowActivityType.End && a.Type != V1WorkflowActivityType.Transition)
                .All(p => p.Status == V1WorkflowActivityStatus.Executed))
            {
                await base.OnCompletedAsync(cancellationToken);
                return;
            }
            foreach (V1WorkflowActivity activity in childActivities
                .Where(p => p.Type == V1WorkflowActivityType.Action && p.Status == V1WorkflowActivityStatus.Pending))
            {
                IWorkflowActivityProcessor nextProcessor = this.CreateProcessorFor(activity);
                await nextProcessor.ProcessAsync(cancellationToken);
            }
        }

    }

}
