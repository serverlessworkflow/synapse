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
using System.Net.Mime;

namespace Synapse.Worker.Services.Processors
{
    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="EventDefinition"/>s to produce
    /// </summary>
    public class ProduceEventProcessor
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
        /// <param name="eventDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the event to produce</param>
        public ProduceEventProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
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
        /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that defines the event to produce
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
                var e = new CloudEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = this.EventDefinition.Type,
                    Source = new(this.EventDefinition.Source!),
                    DataContentType = MediaTypeNames.Application.Json,
                    Data = this.Activity.Input
                };
                if(this.EventDefinition.Correlations != null)
                {
                    foreach (var correlation in this.EventDefinition.Correlations)
                    {
                        var value = correlation.ContextAttributeValue;
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            if (e.TryGetAttribute(correlation.ContextAttributeName, out value))
                                await this.Context.Workflow.SetCorrelationMappingAsync(correlation.ContextAttributeName, value, cancellationToken);
                        }
                        else
                        {
                            if (value.IsRuntimeExpression())
                                value = (await this.Context.EvaluateAsync(value, await this.Context.Workflow.GetActivityStateDataAsync(this.Activity, cancellationToken), cancellationToken)!)!.ToString();
                            await this.Context.Workflow.SetCorrelationMappingAsync(correlation.ContextAttributeName, value!, cancellationToken);
                        }
                        e.SetAttributeFromString(correlation.ContextAttributeName, value!);
                    }
                }
                this.IntegrationEventBus.OutboundStream.OnNext(e);
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, this.Activity.Input), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

    }

}
