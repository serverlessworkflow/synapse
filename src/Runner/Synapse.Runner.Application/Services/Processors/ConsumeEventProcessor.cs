using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventStateTriggerDefinition"/>s
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
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="eventDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="CloudEvent"/> to consume</param>
        public ConsumeEventProcessor(ILoggerFactory loggerFactory, ICloudEventBus cloudEventBus, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, V1WorkflowActivity activity, EventDefinition eventDefinition)
            : base(loggerFactory, executionContext, activityProcessorFactory, activity)
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

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            CloudEvent e = await this.ExecutionContext.GetBoostrapEventAsync(this.EventDefinition, cancellationToken);
            if (e == null)
                this.Subscription = this.CloudEventBus.Subscribe(async e => await this.OnEventAsync(e, cancellationToken));
            else
                await this.OnEventAsync(e, cancellationToken);   
        }

        /// <summary>
        /// Handles an incoming <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnEventAsync(CloudEvent e, CancellationToken cancellationToken)
        {
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.Subscription?.Dispose();
        }

    }

}
