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

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="SwitchStateDefinition"/>s
    /// </summary>
    public class SwitchStateProcessor
        : StateProcessor<SwitchStateDefinition>
    {

        /// <summary>
        /// Gets the name of the default <see cref="SwitchCaseDefinition"/>
        /// </summary>
        public const string DefaultConditionName = "default";

        /// <inheritdoc/>
        public SwitchStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, SwitchStateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {

        }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivity activity)
        {
            var cancellationToken = this.CancellationTokenSource.Token;
            var processor = (ConsumeEventProcessor)base.CreateProcessorFor(activity);
            processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>()
                .SubscribeAsync
            (
                async e => await this.OnConsumeEventResultAsync(processor, e, cancellationToken),
                async ex => await this.OnErrorAsync(ex, cancellationToken)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
            {
                switch (this.State.SwitchType)
                {
                    case SwitchStateType.Data:
                        //Do nothing
                        break;
                    case SwitchStateType.Event:
                        foreach (var eventConditionDefinition in this.State.EventConditions!)
                        {
                            var metadata = new Dictionary<string, string>()
                            {
                                { V1WorkflowActivityMetadata.State, this.State.Name! },
                                { V1WorkflowActivityMetadata.Event, eventConditionDefinition.Event }
                            };
                            await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.ConsumeEvent, this.Activity.Input!.ToObject()!, metadata, this.Activity, cancellationToken);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"The specified switch state '{this.State.SwitchType}' is not supported");
                }
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
            switch (this.State.SwitchType)
            {
                case SwitchStateType.Data:
                    bool caseMatched = false;
                    foreach (var caseDefinition in State.DataConditions!)
                    {
                        if(await this.Context.EvaluateConditionAsync(caseDefinition.Condition, this.Activity.Input!.ToObject()!, cancellationToken))
                        {
                            await this.OnNextAsync(caseDefinition.Name!, cancellationToken);
                            caseMatched = true;
                        }
                    }
                    if (!caseMatched)
                        await this.OnNextAsync(DefaultConditionName, cancellationToken);
                    break;
                case SwitchStateType.Event:
                    foreach (var processor in this.Processors)
                    {
                        await processor.ProcessAsync(cancellationToken);
                    }
                    break;
                default:
                    throw new NotSupportedException($"The specified switch state '{this.State.SwitchType}' is not supported");
            }
        }

        /// <summary>
        /// Produces the <see cref="SwitchStateDefinition"/>'s <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="caseName">The matching <see cref="SwitchCaseDefinition"/>'s name</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async ValueTask OnNextAsync(string caseName, CancellationToken cancellationToken)
        {
            this.Activity.Metadata[V1WorkflowActivityMetadata.Case] = caseName;
            await this.Context.Workflow.SetActivityMetadataAsync(this.Activity, cancellationToken);
            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, this.Activity.Input), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

        /// <summary>
        /// Handles the specified <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="ConsumeEventProcessor"/> that has produced the specified <see cref="V1WorkflowActivityCompletedIntegrationEvent"/></param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnConsumeEventResultAsync(ConsumeEventProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            this.Processors.TryRemove(processor);
            processor.Dispose();
            foreach(var childProcessor in this.Processors)
            {
                await childProcessor.TerminateAsync(cancellationToken);
                this.Processors.TryRemove(childProcessor);
                childProcessor.Dispose();
            }
            if (!this.State.TryGetEventCase(processor.EventDefinition.Name, out var eventCase))
                throw new NullReferenceException($"Failed to find an event case definition that matches the event with the specified name '{processor.EventDefinition.Name}'");
            await this.OnNextAsync(eventCase.Name!, cancellationToken);
        }

    }

}
