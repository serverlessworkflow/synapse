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

namespace Synapse.Runtime.Executor.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="WorkflowActivityProcessor"/> used to process <see cref="ActionDefinition"/>s
    /// </summary>
    public class ActionProcessor
        : WorkflowActivityProcessor, IActionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        public ActionProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivityDto activity, ActionDefinition action)
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the <see cref="FunctionDefinition"/> to process
        /// </summary>
        public ActionDefinition Action { get; }

        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivityDto activity)
        {
            var processor = base.CreateProcessorFor(activity);
            processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
            (
                async e => await this.OnChildActivityCompletedAsync(processor, e, this.CancellationTokenSource.Token),
                async ex => await this.OnErrorAsync(ex, this.CancellationTokenSource.Token),
                async () => await this.OnCompletedAsync(this.CancellationTokenSource.Token)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            var activities = await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken);
            if (!activities.Any())
            {
                if (!string.IsNullOrWhiteSpace(this.Action.Condition)
                && !this.Context.ExpressionEvaluator.EvaluateCondition(this.Action.Condition, this.Activity.Input))
                {
                    await this.OnNextAsync(new V1WorkflowActivitySkippedIntegrationEvent(this.Activity.Id), cancellationToken);
                    return;
                }
                if (this.Action.Sleep != null
                    && this.Action.Sleep.Before.HasValue)
                    await Task.Delay(this.Action.Sleep.Before.Value, cancellationToken);
                var metadata = this.Activity.Metadata;
                switch (this.Action.Type)
                {
                    case ActionType.Function:
                        activities.Add(await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Function, this.Activity.Input, metadata, this.Activity, cancellationToken));
                        break;
                    case ActionType.Subflow:
                        activities.Add(await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Function, this.Activity.Input, metadata, this.Activity, cancellationToken));
                        break;
                    case ActionType.Trigger:
                        activities.Add(await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Function, this.Activity.Input, metadata, this.Activity, cancellationToken));
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionType)} '{this.Action.Type}' is not supported");
                }
            }
            foreach(var activity in activities)
            {
                var processor = this.CreateProcessorFor(activity);
                await processor.ProcessAsync(cancellationToken);
            }
        }

        protected virtual async Task OnChildActivityCompletedAsync(IWorkflowActivityProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            if (this.Action.Sleep != null
                && this.Action.Sleep.After.HasValue)
                await Task.Delay(this.Action.Sleep.After.Value, cancellationToken);
            var output = e.Output.ToObject();
            if (this.Action.ActionDataFilter != null)
                output = this.Context.ExpressionEvaluator.FilterOutput(this.Action, e.Output);
            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
        }

    }

}
