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

using Newtonsoft.Json;
using Synapse.Integration.Events;
using Synapse.Integration.Events.WorkflowActivities;
using Synapse.Integration.Serialization.Converters;
using System.Reactive.Linq;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process <see cref="OperationStateDefinition"/>s
    /// </summary>
    public class OperationStateProcessor
        : StateProcessor<OperationStateDefinition>
    {

        /// <inheritdoc/>
        public OperationStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IJsonSerializer jsonSerializer, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, OperationStateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {
            this.JsonSerializer = jsonSerializer;
        }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            var processor = (ActionProcessor)base.CreateProcessorFor(activity);
            processor.SubscribeAsync
            (
                async e => await this.OnNextActionAsync(processor, e),
                async ex => await this.OnActionFaultedAsync(processor, ex),
                async () => await this.OnActionCompletedAsync(processor)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
                await this.StartAsync();
            else
                await this.ResumeAsync();
        }

        /// <summary>
        /// Starts the <see cref="OperationStateProcessor"/>'s execution
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task StartAsync()
        {
            var input = null as object;
            var metadata = null as Dictionary<string, string>;
            switch (this.State.ActionMode)
            {
                case ActionExecutionMode.Parallel:
                    foreach (var action in this.State.Actions)
                    {
                        input = await this.Context.FilterInputAsync(action, this.Activity.Input, this.CancellationTokenSource.Token);
                        metadata = new()
                            {
                                { V1WorkflowActivityMetadata.State, this.State.Name! },
                                { V1WorkflowActivityMetadata.Action, action.Name! }
                            };
                        await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, input, metadata, this.Activity, this.CancellationTokenSource.Token);
                    }
                    break;
                case ActionExecutionMode.Sequential:
                    var firstAction = this.State.Actions.First();
                    input = await this.Context.FilterInputAsync(firstAction, this.Activity.Input, this.CancellationTokenSource.Token);
                    metadata = new()
                        {
                            { V1WorkflowActivityMetadata.State, this.State.Name! },
                            { V1WorkflowActivityMetadata.Action, firstAction.Name! }
                        };
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, input, metadata, this.Activity, this.CancellationTokenSource.Token);
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.State.ActionMode}' is not supported");
            }
            foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, this.CancellationTokenSource.Token))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <summary>
        /// Resumes the <see cref="OperationStateProcessor"/>'s execution
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ResumeAsync()
        {
            var activities = await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, this.CancellationTokenSource.Token);
            if (activities.Any())
            {
                foreach (var activity in activities)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            else
            {
                switch (this.State.ActionMode)
                {
                    case ActionExecutionMode.Parallel:
                        var output = await this.AggregateActionOutputsAsync(this.CancellationTokenSource.Token);
                        await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), this.CancellationTokenSource.Token);
                        await this.OnCompletedAsync(this.CancellationTokenSource.Token);
                        break;
                    case ActionExecutionMode.Sequential:
                        activities = await this.Context.Workflow.GetActivitiesAsync(this.Activity, this.CancellationTokenSource.Token);
                        var lastActivity = activities.LastOrDefault(a => a.Type == V1WorkflowActivityType.Action);
                        if (lastActivity == null)
                        {
                            await this.StartAsync();
                            return;
                        }
                        if (!this.State.TryGetAction(lastActivity.Metadata, out var lastAction))
                            throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{lastActivity.Id}'");
                        if (!this.State.TryGetNextAction(lastAction.Name!, out var nextAction))
                        {
                            output = await this.AggregateActionOutputsAsync(this.CancellationTokenSource.Token);
                            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), this.CancellationTokenSource.Token);
                            await this.OnCompletedAsync(this.CancellationTokenSource.Token);
                            return;
                        }
                        var metadata = new Dictionary<string, string>()
                        {
                            { V1WorkflowActivityMetadata.State, this.State.Name! },
                            { V1WorkflowActivityMetadata.Action, nextAction.Name! }
                        };
                        var activity = await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, await this.Context.FilterInputAsync(nextAction, lastActivity.Output, this.CancellationTokenSource.Token), metadata, this.Activity, this.CancellationTokenSource.Token);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        this.CreateProcessorFor(activity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.State.ActionMode}' is not supported");
                }
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
        /// Aggregates the output of the completed actions activities that belongs to the processed <see cref="OperationStateDefinition"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The result of the action outputs aggregation</returns>
        protected virtual async Task<object> AggregateActionOutputsAsync(CancellationToken cancellationToken)
        {
            var output = this.Activity.Input.ToObject();
            foreach (var activity in (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                .Where(a => a.Type == V1WorkflowActivityType.Action && a.Status == V1WorkflowActivityStatus.Completed)
                .OrderBy(a => a.ExecutedAt))
            {
                if (activity.Output == null)
                    continue;
                if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Action, out var actionName))
                    throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.Action}'");
                if (!this.State.TryGetAction(actionName, out var action))
                    throw new NullReferenceException($"Failed to find an action with name '{actionName}' in the operation state with name '{this.State.Name}'");
                if (action.UseResults())
                {
                    var expression = action.ActionDataFilter?.ToStateData?.Trim();
                    var json = JsonConvert.SerializeObject(activity.Output, new JsonSerializerSettings() { Converters = new[] { new FilteredExpandoObjectConverter() }, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
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
            return output!;
        }

        /// <summary>
        /// Handles <see cref="V1WorkflowActivity"/>s <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="IActionProcessor"/> to handle the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> for</param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnNextActionAsync(IActionProcessor processor, IV1WorkflowActivityIntegrationEvent e)
        {
            switch (e)
            {
                case V1WorkflowActivitySkippedIntegrationEvent:
                case V1WorkflowActivityCompletedIntegrationEvent:
                    using (await this.Lock.LockAsync(this.CancellationTokenSource.Token))
                    {
                        var output = null as object;
                        switch (this.State.ActionMode)
                        {
                            case ActionExecutionMode.Parallel:
                                var activities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, this.CancellationTokenSource.Token))
                                   .Where(a => a.Type == V1WorkflowActivityType.Action)
                                   .ToList();
                                if (activities.All(a => a.Status >= V1WorkflowActivityStatus.Faulted))
                                {
                                    output = await this.AggregateActionOutputsAsync(this.CancellationTokenSource.Token);
                                    await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), this.CancellationTokenSource.Token);
                                }
                                break;
                            case ActionExecutionMode.Sequential:
                                output = await this.AggregateActionOutputsAsync(this.CancellationTokenSource.Token);
                                if (processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Action, out var currentActionName)
                                     && this.State.TryGetNextAction(currentActionName, out ActionDefinition nextAction))
                                {
                                    var input = await this.Context.FilterInputAsync(nextAction, output, this.CancellationTokenSource.Token);
                                    var metadata = new Dictionary<string, string>()
                                    {
                                        { V1WorkflowActivityMetadata.State, this.State.Name! },
                                        { V1WorkflowActivityMetadata.Action, nextAction.Name! }
                                    };
                                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, input, metadata, this.Activity, this.CancellationTokenSource.Token);
                                }
                                else
                                {
                                    await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), this.CancellationTokenSource.Token);
                                }
                                break;
                            default:
                                throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.State.ActionMode}' is not supported");
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles an <see cref="ActionDefinition"/>'s <see cref="Exception"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has thrown the specified <see cref="Exception"/></param>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionFaultedAsync(ActionProcessor processor, Exception ex)
        {
            using (await this.Lock.LockAsync(this.CancellationTokenSource.Token))
            {
                foreach (var childProcessor in this.Processors)
                {
                    if (childProcessor == processor)
                        continue;
                    await childProcessor.TerminateAsync(this.CancellationTokenSource.Token);
                    this.Processors.TryRemove(processor);
                    childProcessor.Dispose();
                }
                await base.OnErrorAsync(ex, this.CancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has completed</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionCompletedAsync(ActionProcessor processor)
        {
            using (await this.Lock.LockAsync(this.CancellationTokenSource.Token))
            {
                this.Processors.TryRemove(processor);
                processor.Dispose();
                var activities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, this.CancellationTokenSource.Token))
                    .Where(a => a.Type == V1WorkflowActivityType.Action);
                if (activities.All(a => a.Status >= V1WorkflowActivityStatus.Faulted))
                {
                    await base.OnCompletedAsync(this.CancellationTokenSource.Token);
                    return;
                }
                foreach (var activity in activities
                    .Where(p => p.Status == V1WorkflowActivityStatus.Pending))
                {
                    var nextProcessor = this.CreateProcessorFor(activity);
                    await nextProcessor.ProcessAsync(this.CancellationTokenSource.Token);
                }
            }
        }

    }

}
