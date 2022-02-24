﻿using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Events.WorkflowActivities;
using Synapse.Integration.Models;

namespace Synapse.Application.Events.Domain
{
    /// <summary>
    /// Represents the service used to handle <see cref="V1WorkflowActivity"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1WorkflowActivityDomainEventHandler
        : DomainEventHandlerBase<V1WorkflowActivity, V1WorkflowActivityDto, string>,
        INotificationHandler<V1WorkflowActivityCreatedDomainEvent>,
        INotificationHandler<V1WorkflowActivityStartedDomainEvent>,
        INotificationHandler<V1WorkflowActivitySuspendedDomainEvent>,
        INotificationHandler<V1WorkflowActivityResumedDomainEvent>,
        INotificationHandler<V1WorkflowActivityFaultedDomainEvent>,
        INotificationHandler<V1WorkflowActivityCancelledDomainEvent>,
        INotificationHandler<V1WorkflowActivitySkippedDomainEvent>,
        INotificationHandler<V1WorkflowActivityCompletedDomainEvent>
    {

        /// <inheritdoc/>
        public V1WorkflowActivityDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus,
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1WorkflowActivity> aggregates, IRepository<V1WorkflowActivityDto> projections,
            IRepository<V1WorkflowInstanceDto> workflowInstances)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstanceDto"/>
        /// </summary>
        protected IRepository<V1WorkflowInstanceDto> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityStartedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.StartedAt = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Running;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivitySuspendedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Suspended;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityResumedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Running;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityFaultedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.ExecutedAt = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Faulted;
            activity.Error = this.Mapper.Map<ErrorDto>(e.Error);
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityCancelledDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.ExecutedAt = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Cancelled;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivitySkippedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.ExecutedAt = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Skipped;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityCompletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.ExecutedAt = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Completed;
            var outputValue = e.Output as Any;
            if (outputValue == null
                && e.Output != null)
                outputValue = Any.FromObject(e.Output);
            activity.Output = outputValue;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }


        /// <summary>
        /// Updates the specified <see cref="V1WorkflowActivityDto"/> parent <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to update the parent of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual async ValueTask UpdateParentWorkflowInstanceAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken)
        {
            var instance = await this.WorkflowInstances.FindAsync(activity.WorkflowInstanceId, cancellationToken);
            var existingActivity = instance.Activities.FirstOrDefault(a => a.Id == activity.Id);
            if (existingActivity == null)
                instance.Activities.Add(activity);
            else
                this.Mapper.Map(activity, existingActivity);
            await this.WorkflowInstances.UpdateAsync(instance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
        }

    }

}