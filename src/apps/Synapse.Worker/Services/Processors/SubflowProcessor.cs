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

using Synapse.Integration.Events.WorkflowInstances;

namespace Synapse.Worker.Services.Processors
{
    /// <summary>
    /// Represents an <see cref="ActionProcessor"/> used to process subflows
    /// </summary>
    public class SubflowProcessor
        : ActionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="SubflowProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="integrationEventBus">The service used to publish and subscribe to integration events</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="StateDefinition"/> the <see cref="SubflowReference"/> to process belongs to</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        public SubflowProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IIntegrationEventBus integrationEventBus, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, StateDefinition state, ActionDefinition action)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, action)
        {
            this.IntegrationEventBus = integrationEventBus;
            this.State = state;
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
        /// Gets the <see cref="StateDefinition"/> the <see cref="SubflowReference"/> to process belongs to
        /// </summary>
        protected StateDefinition State { get; }

        /// <summary>
        /// Gets the <see cref="SubflowReference"/> to process
        /// </summary>
        protected SubflowReference Subflow => this.Action.Subflow!;

        /// <summary>
        /// Gets the <see cref="Timer"/> used to monitor the time limit before which the current <see cref="V1WorkflowInstance"/> should go into <see cref="V1WorkflowActivityStatus.Waiting"/>
        /// </summary>
        protected Timer IdleTimer { get; private set; } = null!;

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            if (!this.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Subflow, out var workflowInstanceId))
            {
                workflowInstanceId = await this.Context.Workflow.StartSubflowAsync($"{this.Subflow.WorkflowId}:{this.Subflow.Version}", this.Activity.Input, cancellationToken);
                this.Activity.Metadata.Add(V1WorkflowActivityMetadata.Subflow, workflowInstanceId);
                await this.Context.Workflow.SetActivityMetadataAsync(this.Activity, cancellationToken);
            }
            switch (this.Subflow.InvocationMode)
            {
                case InvocationMode.Asynchronous:
                    await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, this.Activity.Input), this.CancellationTokenSource.Token);
                    await this.OnCompletedAsync(this.CancellationTokenSource.Token);
                    break;
                case InvocationMode.Synchronous:
                    var eventDefinition = new EventDefinition()
                    {
                        Kind = EventKind.Consumed,
                        Name = "onSubflowExecuted",
                        Source = CloudEvents.Source.ToString(),
                        Type = CloudEvents.TypeOf(typeof(V1WorkflowInstanceExecutedIntegrationEvent), typeof(V1WorkflowInstance)),
                        Correlations = new()
                        {
                            new() { ContextAttributeName = nameof(CloudEvent.Subject).ToLower(), ContextAttributeValue = workflowInstanceId }
                        }
                    };
                    var e = await this.Context.Workflow.ConsumeOrBeginCorrelateEventAsync(eventDefinition, cancellationToken);
                    if (e != null)
                    {
                        await this.OnEventAsync(e);
                        return;
                    }
                    this.Subscription = this.IntegrationEventBus.InboundStream
                        .Where(e => e.Subject == workflowInstanceId && e.Source?.ToString() == eventDefinition.Source && e.Type == eventDefinition.Type)
                        .SubscribeAsync(this.OnEventAsync);
                    if (this.Options.Correlation.Timeout.HasValue)
                        this.IdleTimer = new Timer(async state => await this.OnIdleTimerExpiredAsync(state, cancellationToken), null, this.Options.Correlation.Timeout.Value, this.Options.Correlation.Timeout.Value);
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(InvocationMode)} '{this.Subflow.InvocationMode}' is not supported");
            }
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
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, e.Data), this.CancellationTokenSource.Token);
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
