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

using Synapse.Application.Commands.Schedules;
using Synapse.Domain.Events.Schedules;
using Synapse.Domain.Events.WorkflowInstances;

namespace Synapse.Application.Events.Domain.v1
{

    /// <summary>
    /// Represents the service used to handle <see cref="V1Schedule"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1ScheduleDomainEventHandler
        : DomainEventHandlerBase<V1Schedule, Integration.Models.V1Schedule, string>,
        INotificationHandler<V1ScheduleCreatedDomainEvent>,
        INotificationHandler<V1ScheduleDefinitionChangedDomainEvent>,
        INotificationHandler<V1ScheduleSuspendedDomainEvent>,
        INotificationHandler<V1ScheduleResumedDomainEvent>,
        INotificationHandler<V1ScheduleOccuredDomainEvent>,
        INotificationHandler<V1ScheduleOccurenceCompletedDomainEvent>,
        INotificationHandler<V1ScheduleRetiredDomainEvent>,
        INotificationHandler<V1ScheduleObsolitedDomainEvent>,
        INotificationHandler<V1ScheduleDeletedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceExecutedDomainEvent>
    {

        /// <inheritdoc/>
        public V1ScheduleDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1Schedule> aggregates, IRepository<Integration.Models.V1Schedule> projections) 
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleDefinitionChangedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.DateTime;
            schedule.Definition = e.Definition;
            schedule.NextOccurenceAt = e.NextOccurenceAt?.Date;
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleSuspendedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.UtcDateTime;
            schedule.SuspendedAt = e.CreatedAt.UtcDateTime;
            schedule.NextOccurenceAt = null;
            schedule.Status = V1ScheduleStatus.Suspended;
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleResumedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.UtcDateTime;
            schedule.NextOccurenceAt = e.NextOccurenceAt?.UtcDateTime;
            schedule.SuspendedAt = null;
            schedule.Status = V1ScheduleStatus.Active;
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleOccuredDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.UtcDateTime;
            schedule.LastOccuredAt = e.CreatedAt.UtcDateTime;
            schedule.NextOccurenceAt = e.NextOccurenceAt?.UtcDateTime;
            schedule.TotalOccurences++;
            if (schedule.ActiveOccurences == null) schedule.ActiveOccurences = new();
            schedule.ActiveOccurences.Add(e.WorkflowInstanceId);
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleOccurenceCompletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.UtcDateTime;
            schedule.LastCompletedAt = e.CreatedAt.UtcDateTime;
            schedule.NextOccurenceAt = e.NextOccurenceAt?.UtcDateTime;
            if (schedule.ActiveOccurences != null) schedule.ActiveOccurences.Remove(e.WorkflowInstanceId);
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleRetiredDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.UtcDateTime;
            schedule.NextOccurenceAt = null;
            schedule.Status = V1ScheduleStatus.Retired;
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleObsolitedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            schedule.LastModified = e.CreatedAt.UtcDateTime;
            schedule.NextOccurenceAt = null;
            schedule.Status = V1ScheduleStatus.Obsolete;
            await this.Projections.UpdateAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ScheduleDeletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.Projections.RemoveAsync(schedule, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceExecutedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var schedule = this.Projections.AsQueryable()
                .FirstOrDefault(s => s.ActiveOccurences != null && s.ActiveOccurences.Contains(e.AggregateId));
            if (schedule == null) return;
            await this.Mediator.ExecuteAndUnwrapAsync(new V1CompleteScheduleOccurenceCommand(schedule.Id, e.AggregateId), cancellationToken);
        }

    }

}
