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
            foreach(var correlation in (await this.Correlations.ToListAsync(cancellationToken))
                .Where(t => t.AppliesTo(command.Event)))
            {
                //todo: log
                var matchingCondition = correlation.GetMatchingConditionFor(command.Event)!;
                var matchingFilter = matchingCondition.GetMatchingFilterFor(command.Event)!;
                switch (correlation.Mode)
                {
                    case V1CorrelationMode.Exclusive:
                        if (correlation.Contexts == null
                            || correlation.Contexts.Count != 1)
                            throw new Exception($"Failed to find the required exclusive context of the correlation with id '{correlation.Id}'");
                        await this.CorrelateAsync(correlation, correlation.Contexts.Single(), command.Event, matchingFilter, cancellationToken);
                        break;
                    case V1CorrelationMode.Parallel:
                        foreach (var context in correlation.Contexts.Where(c => c.CorrelatesTo(command.Event)))
                        {
                            await this.CorrelateAsync(correlation, context, command.Event, matchingFilter, cancellationToken);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(V1CorrelationMode)} '{correlation.Mode}' is not supported");
                }
                await this.Correlations.UpdateAsync(correlation, cancellationToken);
                await this.Correlations.SaveChangesAsync(cancellationToken);
                //todo: log
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
            correlationContext.Correlate(e, filter.Mappings.Keys);
            correlation = await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            //todo: log
            if (!correlation.TryComplete(correlationContext))
            {
                //todo: log
                return;
            }
            //todo: log
            switch (correlation.Outcome.Type)
            {
                case V1CorrelationOutcomeType.Start:
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1CreateWorkflowInstanceCommand(correlation.Outcome.Target, V1WorkflowInstanceActivationType.Trigger, new(), correlationContext, true), cancellationToken);
                    break;
                case V1CorrelationOutcomeType.Resume:
                    await this.Mediator.ExecuteAndUnwrapAsync(new V1CorrelateWorkflowInstanceCommand(correlation.Outcome.Target, correlationContext), cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(V1CorrelationOutcomeType)} '{correlation.Outcome.Type}' is not supported");
            }
            correlation.ReleaseContext(correlationContext);
            correlation = await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
        }

    }

}
