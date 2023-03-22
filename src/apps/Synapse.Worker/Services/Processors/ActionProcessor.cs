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

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="WorkflowActivityProcessor"/> used to process <see cref="ActionDefinition"/>s
    /// </summary>
    public abstract class ActionProcessor
        : WorkflowActivityProcessor, IActionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        protected ActionProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, ActionDefinition action)
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the <see cref="FunctionDefinition"/> to process
        /// </summary>
        public ActionDefinition Action { get; }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            if (this.Action.Sleep != null
             && this.Action.Sleep.Before.HasValue)
                await Task.Delay(this.Action.Sleep.Before.Value, cancellationToken);
            if (string.IsNullOrWhiteSpace(this.Action.Condition)
                || await this.Context.EvaluateConditionAsync(this.Action.Condition, this.Activity.Input.ToObject()!, cancellationToken))
                return;
            this.Logger.LogInformation("Skipping execution of workflow activity with id '{activityId}' because the expression of the processed action with name '{actionName}' evaluated to false.", this.Activity.Id, this.Action.Name);
            this.Activity.Status = V1WorkflowActivityStatus.Skipped;
            await this.OnNextAsync(new V1WorkflowActivitySkippedIntegrationEvent(this.Activity.Id), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task OnNextAsync(IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken)
        {
            if (e is V1WorkflowActivityCompletedIntegrationEvent
                && this.Action.Sleep != null
                && this.Action.Sleep.After.HasValue)
                await Task.Delay(this.Action.Sleep.After.Value, cancellationToken);
            await base.OnNextAsync(e, cancellationToken);
        }

    }

}
