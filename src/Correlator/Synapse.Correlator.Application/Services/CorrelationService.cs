using CloudNative.CloudEvents;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Synapse.Correlator.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ICorrelationService"/> interface
    /// </summary>
    public class CorrelationService
        : BackgroundService, ICorrelationService
    {

        /// <summary>
        /// Initializes a new <see cref="CorrelationService"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="cloudEventBus">The service used to publish and subscribe to <see cref="CloudEvent"/>s</param>
        /// <param name="cloudEventFormatter">The service used to format <see cref="CloudEvent"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="triggers">The <see cref="IRepository{TResource}"/> used to manage <see cref="V1Trigger"/>s</param>
        public CorrelationService(ILogger<CorrelationService> logger, ICloudEventBus cloudEventBus, ICloudEventFormatter cloudEventFormatter, IRepository<V1WorkflowInstance> workflowInstances, IRepository<V1Trigger> triggers)
        {
            this.Logger = logger;
            this.CloudEventBus = cloudEventBus;
            this.CloudEventFormatter = cloudEventFormatter;
            this.WorkflowInstances = workflowInstances;
            this.Triggers = triggers;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to publish and subscribe to <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventBus CloudEventBus { get; }

        /// <summary>
        /// Gets the service used to format <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventFormatter CloudEventFormatter { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TResource}"/> used to manage <see cref="V1Trigger"/>s
        /// </summary>
        protected IRepository<V1Trigger> Triggers { get; }

        /// <summary>
        /// Gets an <see cref="IDisposable"/> that represents the <see cref="CorrelationService"/>'s subscription to the <see cref="ICloudEventBus"/>
        /// </summary>
        protected IDisposable Subscription { get; private set; }

        /// <summary>
        /// Gets the <see cref="Channel{T}"/> used to process incoming <see cref="CloudEvent"/>s
        /// </summary>
        protected Channel<CloudEvent> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<CloudEvent>();

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.Logger.LogInformation(SynapseCorrelatorConstants.Logging.GetHeader());
            this.Subscription = this.CloudEventBus.SubscribeAsync
            (
                async e => await this.Channel.Writer.WriteAsync(e, stoppingToken)
            );
            _ = this.CorrelateAsync(stoppingToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Correlates incoming <see cref="CloudEvent"/>s
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CorrelateAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    CloudEvent e = await this.Channel.Reader.ReadAsync(cancellationToken);
                    if (e == null || cancellationToken.IsCancellationRequested)
                        continue;
                    this.Logger.LogInformation("Processing cloud event with id '{eventId}', type '{eventType}' and source '{eventSource}'...", e.Id, e.Type, e.Source);
                    IEnumerable<V1Trigger> triggers = await this.Triggers.ToListAsync(cancellationToken);
                    int totalTriggerCount = triggers.Count();
                    triggers = triggers.Where(t => t.TriggeredBy(e));
                    int matchingTriggerCount = triggers.Count();
                    this.Logger.LogInformation("Matched the event against {matchingTriggerCount} out of {totalTriggerCount} triggers.", matchingTriggerCount, totalTriggerCount);
                    foreach (V1Trigger trigger in triggers)
                    {
                        this.Logger.LogInformation("Correlating event to trigger '{trigger}'...", trigger.Name());
                        V1TriggerCondition matchingCondition = trigger.GetMatchingConditionFor(e);
                        if (matchingCondition == null)
                        {
                            this.Logger.LogWarning("Event correlation failed"); //should never happen
                            continue;
                        }
                        V1EventFilter matchingFilter = matchingCondition.GetMatchingFilterFor(e);
                        if (matchingFilter == null)
                        {
                            this.Logger.LogWarning("Event correlation failed"); //should never happen
                            continue;
                        }
                        if (trigger.Status == null)
                        {
                            this.Logger.LogInformation("Initializing trigger '{trigger}'...", trigger.Name());
                            trigger.Initialize();
                            await this.Triggers.UpdateAsync(trigger, cancellationToken);
                            this.Logger.LogInformation("Trigger '{trigger}' successfully initialized", trigger.Name());
                        }
                        this.Logger.LogInformation("Retrieving correlation context...");
                        V1CorrelationContext correlationContext = trigger.Status.CorrelationContexts.FirstOrDefault(c => c.CorrelatesTo(e));
                        if (correlationContext == null)
                        {
                            this.Logger.LogInformation("Failed to find a matching correlation context");
                            if (trigger.Status.CorrelationContexts.Any()
                                && trigger.Spec.CorrelationMode != V1TriggerCorrelationMode.Parallel)
                            {
                                this.Logger.LogWarning("Event correlation failed"); //should never happen
                                continue;
                            }
                            this.Logger.LogInformation("Creating a new correlation context...");
                            correlationContext = trigger.CreateCorrelationContextFor(e, matchingFilter.Correlations.Keys, this.CloudEventFormatter);
                            this.Logger.LogInformation("Correlation context with id '{correlationContextId}' successfully created", correlationContext.Id);
                            this.Logger.LogInformation("Event successfully correlated");
                        }
                        else
                        {
                            this.Logger.LogInformation("Correlating event to context with id '{correlationContextId}'...", correlationContext.Id);
                            trigger.Correlate(e, matchingFilter.Correlations.Keys, this.CloudEventFormatter);
                            this.Logger.LogInformation("Event correlated to context with id '{correlationContextId}'", correlationContext.Id);
                            this.Logger.LogInformation("Event successfully correlated");
                        } 
                        await this.Triggers.UpdateAsync(trigger, cancellationToken);
                        this.Logger.LogInformation("Attempting to fire trigger in context with id '{correlationContextId}'...", correlationContext.Id);
                        if (!trigger.TryFireIn(correlationContext))
                        {
                            this.Logger.LogInformation("Trigger conditions are not met in the specified correlation context");
                            continue;
                        }
                        await this.Triggers.UpdateAsync(trigger, cancellationToken);
                        this.Logger.LogInformation("Trigger fired. Computing outcome...");
                        switch (trigger.Spec.Outcome.Type)
                        {
                            case V1TriggerOutcomeType.Run:
                                V1WorkflowInstance workflowInstance = new(new V1WorkflowInstanceSpec(trigger.Spec.Outcome.Workflow, new JObject(), correlationContext))
                                {
                                    Metadata = new V1ObjectMeta()
                                    {
                                        GenerateName = $"{trigger.Spec.Outcome.Workflow.Id}-{trigger.Spec.Outcome.Workflow.Version}-".Slugify("-").ToLower(),
                                        NamespaceProperty = trigger.Namespace()
                                    }
                                };
                                workflowInstance = await this.WorkflowInstances.AddAsync(workflowInstance, cancellationToken);
                                break;
                            case V1TriggerOutcomeType.Resume:
                                //TODO: implement
                                break;
                            default:
                                throw new NotSupportedException($"The specified {nameof(V1TriggerOutcomeType)} '{trigger.Spec.Outcome.Type}' is not supported");
                        }
                        trigger.ReleaseContext(correlationContext);
                        await this.Triggers.UpdateAsync(trigger, cancellationToken);
                        this.Logger.LogInformation("Trigger outcome successfully computed");
                    }
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError($"An error occured while processing an incoming cloud event: {{ex}}", ex.ToString());
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            this.Subscription?.Dispose();
            this.Subscription = null;
        }

    }

}
