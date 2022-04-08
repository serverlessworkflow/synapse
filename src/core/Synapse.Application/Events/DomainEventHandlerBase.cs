using Synapse.Integration.Events;
using System.Net.Mime;

namespace Synapse.Application.Events
{

    /// <summary>
    /// Represents the base class for all <see cref="IDomainEvent"/> handlers
    /// </summary>
    public abstract class DomainEventHandlerBase
    {

        /// <summary>
        /// Initializes a new <see cref="DomainEventHandlerBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="integrationEventBus">The service used to publish <see cref="IIntegrationEvent"/>s</param>
        /// <param name="applicationOptions">The current <see cref="SynapseApplicationOptions"/></param>
        protected DomainEventHandlerBase(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, IOptions<SynapseApplicationOptions> applicationOptions)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Mapper = mapper;
            this.Mediator = mediator;
            this.IntegrationEventBus = integrationEventBus;
            this.ApplicationOptions = applicationOptions.Value;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the service used to publish <see cref="IntegrationEvent"/>s
        /// </summary>
        protected IIntegrationEventBus IntegrationEventBus { get; }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Publishes a <see cref="CloudEvent"/> for the specified <see cref="IDomainEvent"/>
        /// </summary>
        /// <param name="e">The <see cref="IDomainEvent"/> to publish a new <see cref="CloudEvent"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task PublishIntegrationEventAsync<TEvent>(TEvent e, CancellationToken cancellationToken)
            where TEvent : class, IDomainEvent
        {
            if (!e.GetType().TryGetCustomAttribute(out DataTransferObjectTypeAttribute dataTransferObjectTypeAttribute))
                return;
            var integrationEvent = (V1IntegrationEvent)this.Mapper.Map(e, e.GetType(), dataTransferObjectTypeAttribute.Type);
            var aggregateType = e.GetType().GetGenericType(typeof(DomainEvent<,>)).GetGenericArguments()[0];
            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Source = CloudEvents.Source,
                Type = CloudEvents.TypeOf(typeof(TEvent), aggregateType),
                Time = e.CreatedAt,
                Subject = e.AggregateId.ToString(),
                DataSchema = CloudEvents.SchemaOf(typeof(TEvent), aggregateType),
                DataContentType = MediaTypeNames.Application.Json,
                Data = integrationEvent
            };
            await this.IntegrationEventBus.PublishAsync(cloudEvent, cancellationToken);
        }

    }

    /// <summary>
    /// Represents the base class for all <see cref="IDomainEvent"/> handlers
    /// </summary>
    /// <typeparam name="TAggregate">The type of <see cref="IAggregateRoot"/> to handle the <see cref="IDomainEvent"/>s of</typeparam>
    /// <typeparam name="TProjection">The type of projection managed by the <see cref="DomainEventHandlerBase"/></typeparam>
    /// <typeparam name="TKey">The type of key used to uniquely authenticate the <see cref="IAggregateRoot"/>s that produce handled <see cref="IDomainEvent"/>s</typeparam>
    public abstract class DomainEventHandlerBase<TAggregate, TProjection, TKey>
        : DomainEventHandlerBase
        where TAggregate : class, IAggregateRoot<TKey>
        where TProjection : class, IIdentifiable<TKey>
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// Initializes a new <see cref="DomainEventHandlerBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="integrationEventBus">The service used to publish <see cref="IIntegrationEvent"/>s</param>
        /// <param name="synapseOptions">The current <see cref="Configuration.SynapseApplicationOptions"/></param>
        /// <param name="aggregates">The <see cref="IRepository"/> used to manage the <see cref="IAggregateRoot"/>s to handle the <see cref="IDomainEvent"/>s of</param>
        /// <param name="projections">The <see cref="IRepository"/> used to manage the <see cref="DomainEventHandlerBase"/>'s projections</param>
        protected DomainEventHandlerBase(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, 
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<TAggregate> aggregates, IRepository<TProjection> projections)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions)
        {
            this.Aggregates = aggregates;
            this.Projections = projections;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage the <see cref="IAggregateRoot"/>s to handle the <see cref="IDomainEvent"/>s of
        /// </summary>
        protected IRepository<TAggregate> Aggregates { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage the <see cref="DomainEventHandlerBase"/>'s projections
        /// </summary>
        protected IRepository<TProjection> Projections { get; }

        /// <summary>
        /// Gets or reconciles the projection for the <see cref="IAggregateRoot"/> with the specified key
        /// </summary>
        /// <param name="aggregateKey">The id of the <see cref="IAggregateRoot"/> to get the projection for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The projection for the <see cref="IAggregateRoot"/> with the specified key</returns>
        protected virtual async Task<TProjection> GetOrReconcileProjectionAsync(TKey aggregateKey, CancellationToken cancellationToken)
        {
            TProjection projection = await this.Projections.FindAsync(aggregateKey, cancellationToken);
            if (projection == null)
            {
                TAggregate aggregate = await this.Aggregates.FindAsync(aggregateKey, cancellationToken);
                if (aggregate == null)
                {
                    this.Logger.LogError("Failed to find an aggregate of type '{aggregateType}' with the specified key '{key}'", typeof(TAggregate), aggregateKey);
                    throw new Exception($"Failed to find an aggregate of type '{typeof(TAggregate)}' with the specified key '{aggregateKey}'");
                }
                projection = await this.ProjectAsync(aggregate, cancellationToken);
                projection = await this.Projections.AddAsync(projection, cancellationToken);
                await this.Projections.SaveChangesAsync(cancellationToken);
            }
            return projection;
        }

        /// <summary>
        /// Projects the specified <see cref="IAggregateRoot"/>
        /// </summary>
        /// <param name="aggregate">The <see cref="IAggregateRoot"/> to project</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The projected <see cref="IAggregateRoot"/></returns>
        protected virtual async Task<TProjection> ProjectAsync(TAggregate aggregate, CancellationToken cancellationToken)
        {
            return await Task.FromResult(this.Mapper.Map<TProjection>(aggregate));
        }

    }

}
