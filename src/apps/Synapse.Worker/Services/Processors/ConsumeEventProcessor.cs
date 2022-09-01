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
using Synapse.Integration.Events.WorkflowActivities;
using System.Text.RegularExpressions;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventDefinition"/>s to consume
    /// </summary>
    public class ConsumeEventProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="integrationEventBus">The service used to publish and subscribe to integration events</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="eventDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="V1Event"/> to consume</param>
        public ConsumeEventProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, 
            IIntegrationEventBus integrationEventBus, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, EventDefinition eventDefinition) 
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.IntegrationEventBus = integrationEventBus;
            this.EventDefinition = eventDefinition;
        }

        /// <summary>
        /// Gets the service used to publish and subscribe to integration events
        /// </summary>
        protected IIntegrationEventBus IntegrationEventBus { get; }

        /// <summary>
        /// Gets the <see cref="CloudEvent"/> subscription
        /// </summary>
        protected IDisposable Subscription { get; private set; } = null!;

        /// <summary>
        /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="V1Event"/> to consume
        /// </summary>
        public EventDefinition EventDefinition { get; }

        /// <summary>
        /// Gets the <see cref="Timer"/> used to monitor the time limit before which the current <see cref="V1WorkflowInstance"/> should go into <see cref="V1WorkflowActivityStatus.Waiting"/>
        /// </summary>
        protected Timer IdleTimer { get; private set; } = null!;

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.EventDefinition.Correlations != null)
            {
                foreach(var correlation in this.EventDefinition.Correlations)
                {
                    var value = correlation.ContextAttributeValue;
                    if (!string.IsNullOrWhiteSpace(value)
                        && value.IsRuntimeExpression())
                        value = (await this.Context.EvaluateAsync(value, this.Activity.Input.ToObject()!, cancellationToken))!.ToString();
                    await this.Context.Workflow.SetCorrelationMappingAsync(correlation.ContextAttributeName, value!, cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            this.Subscription = this.IntegrationEventBus.InboundStream.SubscribeAsync(this.OnEventAsync);
            var e = await this.Context.Workflow.ConsumeOrBeginCorrelateEventAsync(this.EventDefinition, cancellationToken);
            if (e != null)
            {
                await this.OnEventAsync(e);
                return;
            }
            if (this.Options.Correlation.Timeout.HasValue)
                this.IdleTimer = new Timer(async state => await this.OnIdleTimerExpiredAsync(state, cancellationToken), null, this.Options.Correlation.Timeout.Value, this.Options.Correlation.Timeout.Value);
        }

        /// <summary>
        /// Switches to passive correlation mode
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task SwitchToPassiveModeAsync(CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                //todo: passive correlation mode: await this.Context.WaitForEventsAsync(new[] { this.EventDefinition }, V1TriggerCorrelationMode.Exclusive, V1TriggerConditionType.AllOf, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the expiration of the iddle <see cref="Timer"/>
        /// </summary>
        /// <param name="state">The <see cref="Timer"/>'s state</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnIdleTimerExpiredAsync(object? state, CancellationToken cancellationToken)
        {
            this.IdleTimer.Dispose();
            this.IdleTimer = null!;
            if (this.Options.Correlation.Mode == RuntimeCorrelationMode.Dual)
            {
                await this.SwitchToPassiveModeAsync(cancellationToken);
            }
            else
            {
                //TODO
                //await this.OnTimeoutAsync();
            }
        }

        /// <summary>
        /// Handles an incoming <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEventAsync(CloudEvent e)
        {
            using (await this.Lock.LockAsync(this.CancellationTokenSource.Token))
            {
                if (this.IdleTimer != null)
                {
                    this.IdleTimer.Dispose();
                    this.IdleTimer = null!;
                }
                if ((!string.IsNullOrWhiteSpace(this.EventDefinition.Source) && !Regex.IsMatch(e.Source!.ToString(), this.EventDefinition.Source, RegexOptions.IgnoreCase))
                    || (!string.IsNullOrWhiteSpace(this.EventDefinition.Type) && !Regex.IsMatch(e.Type!, this.EventDefinition.Type, RegexOptions.IgnoreCase)))
                    return;
                var enveloppe = V1Event.CreateFrom(e);
                if (!await this.Context.Workflow.TryCorrelateAsync(enveloppe, this.EventDefinition.Correlations?.Select(c => c.ContextAttributeName)!, this.CancellationTokenSource.Token))
                    return;
                object output;
                if (this.EventDefinition.DataOnly)
                    output = e.Data!;
                else
                    output = enveloppe;
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), this.CancellationTokenSource.Token);
                await this.OnCompletedAsync(this.CancellationTokenSource.Token);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.IdleTimer?.Dispose();
            this.Subscription?.Dispose();
            this.Subscription?.Dispose();
        }

    }

}
