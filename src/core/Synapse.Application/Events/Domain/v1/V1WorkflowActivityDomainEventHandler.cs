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
using Neuroglia.Serialization;
using Synapse.Domain.Events.WorkflowActivities;

namespace Synapse.Application.Events.Domain
{
    /// <summary>
    /// Represents the service used to handle <see cref="V1WorkflowActivity"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1WorkflowActivityDomainEventHandler
        : DomainEventHandlerBase<V1WorkflowActivity, Integration.Models.V1WorkflowActivity, string>,
        INotificationHandler<V1WorkflowActivityCreatedDomainEvent>,
        INotificationHandler<V1WorkflowActivityStartedDomainEvent>,
        INotificationHandler<V1WorkflowActivitySuspendedDomainEvent>,
        INotificationHandler<V1WorkflowActivityResumedDomainEvent>,
        INotificationHandler<V1WorkflowActivityFaultedDomainEvent>,
        INotificationHandler<V1WorkflowActivityCompensatingDomainEvent>,
        INotificationHandler<V1WorkflowActivityCompensatedDomainEvent>,
        INotificationHandler<V1WorkflowActivityCancelledDomainEvent>,
        INotificationHandler<V1WorkflowActivitySkippedDomainEvent>,
        INotificationHandler<V1WorkflowActivityCompletedDomainEvent>,
        INotificationHandler<V1WorkflowActivityExecutedDomainEvent>
    {

        /// <inheritdoc/>
        public V1WorkflowActivityDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus,
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1WorkflowActivity> aggregates, IRepository<Integration.Models.V1WorkflowActivity> projections,
            IRepository<Integration.Models.V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Integration.Models.V1WorkflowInstance"/>
        /// </summary>
        protected IRepository<Integration.Models.V1WorkflowInstance> WorkflowInstances { get; }

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
            activity.Error = this.Mapper.Map<Integration.Models.Error>(e.Error);
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityCompensatingDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Compensating;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityCompensatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var activity = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            activity.LastModified = e.CreatedAt.UtcDateTime;
            activity.Status = V1WorkflowActivityStatus.Compensated;
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
            var outputValue = e.Output as Dynamic;
            if (outputValue == null
                && e.Output != null)
                outputValue = Dynamic.FromObject(e.Output);
            activity.Output = outputValue;
            await this.Projections.UpdateAsync(activity, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.UpdateParentWorkflowInstanceAsync(activity, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityExecutedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <summary>
        /// Updates the specified <see cref="Integration.Models.V1WorkflowActivity"/> parent <see cref="V1WorkflowInstance"/>
        /// </summary>
        /// <param name="activity">The <see cref="Integration.Models.V1WorkflowActivity"/> to update the parent of</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual async ValueTask UpdateParentWorkflowInstanceAsync(Integration.Models.V1WorkflowActivity activity, CancellationToken cancellationToken)
        {
            var instance = await this.WorkflowInstances.FindAsync(activity.WorkflowInstanceId, cancellationToken);
            Integration.Models.V1WorkflowActivity existingActivity = instance.Activities.FirstOrDefault(a => a.Id == activity.Id)!;
            if (existingActivity == null)
            {
                instance.Activities.Add(activity);
            } 
            else
            {
                var list = instance.Activities.ToList();
                var index = list.IndexOf(existingActivity);
                existingActivity = this.Mapper.Map(activity, existingActivity);
                list[index] = existingActivity;
                instance.Activities = list;
            }
            await this.WorkflowInstances.UpdateAsync(instance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
        }

    }

}
