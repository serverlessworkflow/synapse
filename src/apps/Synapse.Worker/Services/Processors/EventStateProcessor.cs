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
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventStateDefinition"/>s
    /// </summary>
    public class EventStateProcessor
        : StateProcessor<EventStateDefinition>
    {

        /// <inheritdoc/>
        public EventStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, EventStateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {

        }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            var processor = (EventStateTriggerProcessor)base.CreateProcessorFor(activity);
            processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
            (
                async result => await this.OnTriggerResultAsync(processor, result, this.CancellationTokenSource.Token),
                async ex => await this.OnErrorAsync(ex, this.CancellationTokenSource.Token),
                async () => await this.OnTriggerCompletedAsync(processor, this.CancellationTokenSource.Token)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status <= V1WorkflowActivityStatus.Pending)
            {
                var metadata = new Dictionary<string, string>()
                {
                    { V1WorkflowActivityMetadata.State, this.State.Name }
                };
                foreach (var trigger in this.State.Triggers)
                {
                    metadata[V1WorkflowActivityMetadata.Trigger] = this.State.Triggers.IndexOf(trigger).ToString();
                    await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.EventTrigger, this.Activity.Input, metadata, this.Activity, cancellationToken);
                }
            }
            foreach (var childActivity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.CreateProcessorFor(childActivity);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (IWorkflowActivityProcessor childProcessor in this.Processors.ToList())
            {
                await childProcessor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Handles the next <see cref="IWorkflowActivityProcessor"/>'s <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> that returned the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnTriggerResultAsync(IWorkflowActivityProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                if (this.Activity.Status == V1WorkflowActivityStatus.Completed)
                    return;
                var completedActivities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                    .Where(p => p.Type == V1WorkflowActivityType.EventTrigger && p.Status == V1WorkflowActivityStatus.Completed)
                    .ToList();
                if (this.State.Exclusive
                    || completedActivities.Count == this.State.Triggers.Count)
                {
                    var output = new object();
                    foreach (var activity in completedActivities
                        .Where(p => p.Output != null))
                    {
                        output = output.Merge(activity.Output.ToObject());
                    }
                    await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                    await this.OnCompletedAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="IWorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="processor">The <see cref="IWorkflowActivityProcessor"/> to handle the completion of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual Task OnTriggerCompletedAsync(IWorkflowActivityProcessor processor, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            return Task.CompletedTask;
        }

    }

}
