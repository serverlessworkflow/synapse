using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using Synapse.Services;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Runner.Application.Services.Processors
{
    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventDefinition"/>s to produce
    /// </summary>
    public class ProduceEventProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="ProduceEventProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="cloudEventBus">The service used to publish and subscribe to <see cref="CloudEvent"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="eventDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="CloudEvent"/> to produce</param>
        public ProduceEventProcessor(ILoggerFactory loggerFactory, ICloudEventBus cloudEventBus, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, EventDefinition eventDefinition)
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
        /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the <see cref="CloudEvent"/> to produce
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
            try
            {
                CloudEvent e = new(this.EventDefinition.Type, new Uri(this.EventDefinition.Source, UriKind.RelativeOrAbsolute))
                {
                    DataContentType = new ContentType(MediaTypeNames.Application.Json),
                    Data = this.Activity.Data
                };
                foreach (EventCorrelationDefinition correlation in this.EventDefinition.Correlations)
                {
                    string value = correlation.ContextAttributeValue;
                    if(string.IsNullOrWhiteSpace(value))
                    {
                        if (e.TryGetAttribute(correlation.ContextAttributeName, out value))
                            await this.ExecutionContext.SetCorrelationKeyAsync(correlation.ContextAttributeName, value, cancellationToken);
                    }
                    else
                    {
                        if (value.IsWorkflowExpression())
                            value = this.ExecutionContext.ExpressionEvaluator.Evaluate(value, await this.ExecutionContext.GetActivityStateDataAsync(this.Activity, cancellationToken)).Value<string>();
                        await this.ExecutionContext.SetCorrelationKeyAsync(correlation.ContextAttributeName, value, cancellationToken);
                    }
                    e.GetAttributes()[correlation.ContextAttributeName] = value;
                }
                await this.CloudEventBus.PublishAsync(e, cancellationToken);
                await this.OnResultAsync(V1WorkflowExecutionResult.Next(this.Activity.Data), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

    }

}
