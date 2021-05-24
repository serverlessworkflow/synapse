using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using Synapse.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventDefinition"/>s to consume
    /// </summary>
    public class ConsumeEventProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="ConsumeEventProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="cloudEventBus">The service used to publish and subscribe to <see cref="CloudEvent"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="eventDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="CloudEvent"/> to consume</param>
        public ConsumeEventProcessor(ILoggerFactory loggerFactory, ICloudEventBus cloudEventBus, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, EventDefinition eventDefinition)
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity)
        {
            this.CloudEventBus = cloudEventBus;
            this.EventDefinition = eventDefinition;
        }

        /// <summary>
        /// Gets the service used to publish and subscribe to <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventBus CloudEventBus { get; }

        /// <summary>
        /// Gets the <see cref="CloudEvent"/> subscription
        /// </summary>
        protected IDisposable Subscription { get; private set; }

        /// <summary>
        /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="CloudEvent"/> to consume
        /// </summary>
        public EventDefinition EventDefinition { get; }

        /// <summary>
        /// Gets the <see cref="Timer"/> used to monitor the time limit before which the current <see cref="V1WorkflowInstance"/> should go into <see cref="V1WorkflowActivityStatus.Waiting"/>
        /// </summary>
        protected Timer IdleTimer { get; private set; }

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            CloudEvent e = await this.ExecutionContext.GetBoostrapEventAsync(this.EventDefinition, cancellationToken);
            if (e != null)
            {
                await this.OnEventAsync(e, cancellationToken);
                return;
            }
            switch (this.Options.Runtime.Correlation.Mode)
            {
                case RuntimeCorrelationMode.Active:
                case RuntimeCorrelationMode.Dual:
                    if (this.Options.Runtime.Correlation.Timeout.HasValue)
                        this.IdleTimer = new Timer(async state => await this.OnIdleTimerExpiredAsync(state, cancellationToken), null, this.Options.Runtime.Correlation.Timeout.Value, this.Options.Runtime.Correlation.Timeout.Value);
                    this.Subscription = this.CloudEventBus.Subscribe(async e => await this.OnEventAsync(e, cancellationToken));
                    break;
                case RuntimeCorrelationMode.Passive:
                    await this.SwitchToPassiveModeAsync(cancellationToken);
                    break;
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
                await this.ExecutionContext.WaitForEventsAsync(new[] { this.EventDefinition }, V1TriggerCorrelationMode.Exclusive, V1TriggerConditionType.AllOf, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the expiration of the iddle <see cref="Timer"/>
        /// </summary>
        /// <param name="state">The <see cref="Timer"/>'s state</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnIdleTimerExpiredAsync(object state, CancellationToken cancellationToken)
        {
            this.IdleTimer.Dispose();
            this.IdleTimer = null;
            if(this.Options.Runtime.Correlation.Mode == RuntimeCorrelationMode.Dual)
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEventAsync(CloudEvent e, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                if (this.IdleTimer != null)
                {
                    this.IdleTimer.Dispose();
                    this.IdleTimer = null;
                }
                if (!string.IsNullOrWhiteSpace(this.EventDefinition.Source) && !Regex.IsMatch(e.Source.ToString(), this.EventDefinition.Source)
                    || !string.IsNullOrWhiteSpace(this.EventDefinition.Type) && !Regex.IsMatch(e.Type, this.EventDefinition.Type))
                    return;
                if (!await this.ExecutionContext.TryCorrelateAsync(e, this.EventDefinition.Correlations?.Select(c => c.ContextAttributeName), cancellationToken))
                    return;
                JToken output;
                if (e.Data != null)
                    if (e.Data is JObject jobject)
                        output = jobject;
                    else if (e.Data is string json)
                        output = JToken.Parse(json);
                    else
                        output = JToken.FromObject(e.Data);
                else
                    output = new JObject();
                await this.OnResultAsync(V1WorkflowExecutionResult.Next(output), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.IdleTimer?.Dispose();
            this.Subscription?.Dispose();
        }

    }

}
