using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Events.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Application.Events.Domain
{

    /// <summary>
    /// Represents the service used to handle <see cref="V1WorkflowInstance"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1WorkflowInstanceDomainEventHandler
        : DomainEventHandlerBase<V1WorkflowInstance, V1WorkflowInstanceDto, string>,
        INotificationHandler<V1WorkflowInstanceCreatedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceSchedulingDomainEvent>,
        INotificationHandler<V1WorkflowInstanceScheduledDomainEvent>,
        INotificationHandler<V1WorkflowInstanceStartingDomainEvent>,
        INotificationHandler<V1WorkflowInstanceStartedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceSuspendingDomainEvent>,
        INotificationHandler<V1WorkflowInstanceSuspendedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceResumingDomainEvent>,
        INotificationHandler<V1WorkflowInstanceResumedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceFaultedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceCancellingDomainEvent>,
        INotificationHandler<V1WorkflowInstanceCancelledDomainEvent>,
        INotificationHandler<V1WorkflowInstanceCompletedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceDeletedDomainEvent>
    {

        /// <inheritdoc/>
        public V1WorkflowInstanceDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus,
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1WorkflowInstance> aggregates, IRepository<V1WorkflowInstanceDto> projections)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceSchedulingDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Scheduling;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceScheduledDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Scheduled;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceStartingDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Starting;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceStartedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.StartedAt = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Running;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceSuspendingDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Suspending;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceSuspendedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Suspended;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceResumingDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Resuming;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceResumedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Running;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceFaultedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.ExecutedAt = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Faulted;
            instance.Error = this.Mapper.Map<ErrorDto>(e.Error);
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceCancellingDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Cancelling;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceCancelledDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.ExecutedAt = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Cancelled;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceCompletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            instance.LastModified = e.CreatedAt.UtcDateTime;
            instance.ExecutedAt = e.CreatedAt.UtcDateTime;
            instance.Status = V1WorkflowInstanceStatus.Completed;
            var outputValue = e.Output as Any;
            if (outputValue == null
                && e.Output != null)
                outputValue = Any.FromObject(e.Output);
            instance.Output = outputValue;
            await this.Projections.UpdateAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceDeletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var instance = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.Projections.RemoveAsync(instance, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }
 
    }

}
