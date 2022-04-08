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

using Synapse.Application.Commands.WorkflowInstances;

namespace Synapse.Application.Commands.Correlations
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to correlate a <see cref="V1Event"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Correlations.V1CorrelateEventCommand))]
    public class V1CorrelateEventCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelateEventCommand"/>
        /// </summary>
        protected V1CorrelateEventCommand()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1CorrelateEventCommand"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to correlate</param>
        public V1CorrelateEventCommand(V1Event e)
        {
            this.Event = e;
        }

        /// <summary>
        /// Gets the <see cref="V1Event"/> to correlate
        /// </summary>
        public virtual V1Event Event { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CorrelateEventCommand"/>s
    /// </summary>
    public class V1CorrelateEventCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CorrelateEventCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CorrelateEventCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="correlations">The <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        public V1CorrelateEventCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IRepository<V1Correlation> correlations, IRepository<V1WorkflowInstance> workflowInstances) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Correlations = correlations;
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s
        /// </summary>
        protected IRepository<V1Correlation> Correlations { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1CorrelateEventCommand command, CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Processing event with id '{eventId}', type '{eventType}' and source '{eventSource}'...", command.Event.Id, command.Event.Type, command.Event.Source);
            IEnumerable<V1Correlation> correlations = await this.Correlations.ToListAsync(cancellationToken);
            int totalTriggerCount = correlations.Count();
            correlations = correlations.Where(c => c.AppliesTo(command.Event));
            int matchingTriggerCount = correlations.Count();
            this.Logger.LogInformation("Matched the event against {matchingCorrelationCount} out of {totalCorrelationCount} correlations.", matchingTriggerCount, totalTriggerCount);
            foreach (var correlation in correlations)
            {
                this.Logger.LogInformation("Processing correlation with id '{correlationId}'...", correlation.Id);
                var matchingCondition = correlation.GetMatchingConditionFor(command.Event)!;
                var matchingFilter = matchingCondition.GetMatchingFilterFor(command.Event)!;
                var matchingContexts = correlation.Contexts
                    .Where(c => c.CorrelatesTo(command.Event))
                    .ToList();
                switch (correlation.Lifetime)
                {
                    case V1CorrelationLifetime.Singleton:
                        var matchingContext = matchingContexts.FirstOrDefault();
                        if (matchingContext == null)
                        {
                            this.Logger.LogInformation("Failed to find a matching correlation context");
                            if (correlation.Contexts.Any())
                                throw new Exception("Failed to correlate event"); //should not happen
                            this.Logger.LogInformation("Creating a new correlation context...");
                            matchingContext = V1CorrelationContext.CreateFor(command.Event, matchingFilter.CorrelationMappings.Keys);
                            correlation.AddContext(matchingContext);
                            await this.Correlations.UpdateAsync(correlation, cancellationToken);
                            await this.Correlations.SaveChangesAsync(cancellationToken);
                            this.Logger.LogInformation("Correlation context with id '{contextId}' successfully created", matchingContext.Id);
                            this.Logger.LogInformation("Event successfully correlated to context with id '{contextId}'", matchingContext.Id);
                        }
                        else
                        {
                            await this.CorrelateAsync(correlation, matchingContext, command.Event, matchingFilter, cancellationToken);
                        }
                        break;
                    case V1CorrelationLifetime.Transient:
                        matchingContext = matchingContexts.FirstOrDefault();
                        if(matchingContext == null)
                        {
                            this.Logger.LogInformation("Failed to find a matching correlation context");
                            this.Logger.LogInformation("Creating a new correlation context...");
                            matchingContext = V1CorrelationContext.CreateFor(command.Event, matchingFilter.CorrelationMappings.Keys);
                            correlation.AddContext(matchingContext);
                            await this.Correlations.UpdateAsync(correlation, cancellationToken);
                            await this.Correlations.SaveChangesAsync(cancellationToken);
                            this.Logger.LogInformation("Correlation context with id '{contextId}' successfully created", matchingContext.Id);
                            this.Logger.LogInformation("Event successfully correlated to context with id '{contextId}'", matchingContext.Id);
                            matchingContexts.Add(matchingContext);
                        }
                        this.Logger.LogInformation("Found {matchingContextCount} matching correlation contexts", matchingContexts.Count());
                        foreach (var context in matchingContexts)
                        {
                            await this.CorrelateAsync(correlation, context, command.Event, matchingFilter, cancellationToken);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(V1CorrelationLifetime)} '{correlation.Lifetime}' is not supported");
                }
            }
            return this.Ok();
        }

        /// <summary>
        /// Performs a correlation on a <see cref="V1Event"/>, in a given <see cref="V1CorrelationContext"/> and using the specified <see cref="V1EventFilter"/>
        /// </summary>
        /// <param name="correlation">The <see cref="V1Correlation"/> to perform</param>
        /// <param name="correlationContext">The <see cref="V1CorrelationContext"/> in which to perform the <see cref="V1Correlation"/></param>
        /// <param name="e">The <see cref="V1Event"/> to correlate</param>
        /// <param name="filter">The <see cref="V1EventFilter"/> used to correlate the <see cref="V1Event"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CorrelateAsync(V1Correlation correlation, V1CorrelationContext correlationContext, V1Event e, V1EventFilter filter, CancellationToken cancellationToken = default)
        {
            this.Logger.LogInformation("Correlating event to context with id '{contextId}'...", correlationContext.Id);
            correlationContext.Correlate(e, filter.CorrelationMappings.Keys, true);
            correlation = await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            this.Logger.LogInformation("Event successfully correlated to context with id '{contextId}'", correlationContext.Id);
            this.Logger.LogInformation("Attempting to complete the correlation with id '{correlationId}' in context with id '{contextId}'...", correlation.Id, correlationContext.Id);
            if (!correlation.TryComplete(correlationContext))
            {
                this.Logger.LogInformation("Correlations conditions are not met in the specified correlation context");
                return;
            }
            this.Logger.LogInformation("Correlation with id '{correlationId}' has been completed in context with id '{contextId}. Computing outcome...", correlation.Id, correlationContext.Id);
            switch (correlation.Outcome.Type)
            {
                case V1CorrelationOutcomeType.Start:
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1CreateWorkflowInstanceCommand(correlation.Outcome.Target, V1WorkflowInstanceActivationType.Trigger, new(), correlationContext, true, null), cancellationToken);
                    break;
                case V1CorrelationOutcomeType.Correlate:
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1CorrelateWorkflowInstanceCommand(correlation.Outcome.Target, correlationContext), cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(V1CorrelationOutcomeType)} '{correlation.Outcome.Type}' is not supported");
            }
            correlation.ReleaseContext(correlationContext);
            correlation = await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            if(correlation.Lifetime == V1CorrelationLifetime.Singleton)
            {
                this.Logger.LogInformation("The correlation with id '{correlationId}' is a singleton and its context has been released. Disposing of it...", correlation.Id);
                await this.Correlations.RemoveAsync(correlation, cancellationToken);
                await this.Correlations.SaveChangesAsync(cancellationToken);
                this.Logger.LogInformation("The correlation with id '{correlationId}' has been successfully disposed of", correlation.Id);
            }
            this.Logger.LogInformation("Correlation outcome successfully computed");
        }

    }

}
