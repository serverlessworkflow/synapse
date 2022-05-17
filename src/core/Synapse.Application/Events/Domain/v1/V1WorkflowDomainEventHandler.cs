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

using Synapse.Application.Queries.Generic;
using Synapse.Domain.Events.WorkflowInstances;
using Synapse.Domain.Events.Workflows;

namespace Synapse.Application.Events.Domain
{

    /// <summary>
    /// Represents the service used to handle <see cref="V1Workflow"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1WorkflowDomainEventHandler
        : DomainEventHandlerBase<V1Workflow, Integration.Models.V1Workflow, string>,
        INotificationHandler<V1WorkflowCreatedDomainEvent>,
        INotificationHandler<V1WorkflowInstanciatedDomainEvent>,
        INotificationHandler<V1WorkflowDeletedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceStartedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceExecutedDomainEvent>
    {

        /// <inheritdoc/>
        public V1WorkflowDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, 
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1Workflow> aggregates, IRepository<Integration.Models.V1Workflow> projections) 
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanciatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var workflow = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            workflow.LastModified = e.CreatedAt.UtcDateTime;
            workflow.LastInstanciated = e.CreatedAt.UtcDateTime;
            workflow.TotalInstanceCount++;
            await this.Projections.UpdateAsync(workflow, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowDeletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var workflow = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.Projections.RemoveAsync(workflow, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceStartedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.Mediator.ExecuteAndUnwrapAsync(new V1FindByIdQuery<Integration.Models.V1WorkflowInstance, string>(e.AggregateId));
            var workflow = await this.GetOrReconcileProjectionAsync(workflowInstance.WorkflowId, cancellationToken);
            workflow.RunningInstanceCount++;
            await this.Projections.UpdateAsync(workflow, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceExecutedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.Mediator.ExecuteAndUnwrapAsync(new V1FindByIdQuery<Integration.Models.V1WorkflowInstance, string>(e.AggregateId));
            var workflow = await this.GetOrReconcileProjectionAsync(workflowInstance.WorkflowId, cancellationToken);
            workflow.RunningInstanceCount--;
            workflow.ExecutedInstanceCount++;
            switch (e.Status)
            {
                case V1WorkflowInstanceStatus.Completed:
                    workflow.CompletedInstanceCount++;
                    break;
                case V1WorkflowInstanceStatus.Faulted:
                    workflow.FaultedInstanceCount++;
                    break;
                case V1WorkflowInstanceStatus.Cancelled:
                    workflow.CancelledInstanceCount++;
                    break;
            }
            if (!workflowInstance.ExecutedAt.HasValue)
                workflowInstance.ExecutedAt = e.CreatedAt.DateTime;
            workflow.TotalInstanceExecutionTime = workflow.TotalInstanceExecutionTime .Add(workflowInstance.Duration!.Value);
            if (!workflow.ShortestInstanceDuration.HasValue
                || workflowInstance.Duration < workflow.ShortestInstanceDuration)
                workflow.ShortestInstanceDuration = workflowInstance.Duration;
            if (!workflow.LongestInstanceDuration.HasValue
                || workflowInstance.Duration > workflow.LongestInstanceDuration)
                workflow.LongestInstanceDuration = workflowInstance.Duration;
            await this.Projections.UpdateAsync(workflow, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
        }

    }

}
