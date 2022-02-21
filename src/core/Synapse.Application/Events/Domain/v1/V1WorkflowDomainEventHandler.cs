using Synapse.Domain.Events.Workflows;
using Synapse.Integration.Models;

namespace Synapse.Application.Events.Domain
{

    /// <summary>
    /// Represents the service used to handle <see cref="V1Workflow"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1WorkflowDomainEventHandler
        : DomainEventHandlerBase<V1Workflow, V1WorkflowDto, string>,
        INotificationHandler<V1WorkflowCreatedDomainEvent>
    {

        /// <inheritdoc/>
        public V1WorkflowDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, 
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1Workflow> aggregates, IRepository<V1WorkflowDto> projections) 
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

    }

}
