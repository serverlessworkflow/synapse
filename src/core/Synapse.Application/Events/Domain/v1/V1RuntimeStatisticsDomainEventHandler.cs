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

using Synapse.Domain.Events.WorkflowActivities;
using Synapse.Domain.Events.WorkflowInstances;
using Synapse.Domain.Events.Workflows;
using Synapse.Integration.Models;

namespace Synapse.Application.Events.Domain
{

    /// <summary>
    /// Represents the service used to handle <see cref="IDomainEvent"/>s projected by <see cref="V1ApplicationMetrics"/>
    /// </summary>
    public class V1RuntimemetricsDomainEventHandler
        : DomainEventHandlerBase,
        INotificationHandler<V1WorkflowCreatedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceCreatedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceStartedDomainEvent>,
        INotificationHandler<V1WorkflowInstanceExecutedDomainEvent>,
        INotificationHandler<V1WorkflowActivityCreatedDomainEvent>,
        INotificationHandler<V1WorkflowActivityStartedDomainEvent>,
        INotificationHandler<V1WorkflowActivityExecutedDomainEvent>
    {

        /// <summary>
        /// Initializes a new <see cref="V1RuntimemetricsDomainEventHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="integrationEventBus">The service used to publish <see cref="IIntegrationEvent"/>s</param>
        /// <param name="applicationOptions">The current <see cref="SynapseApplicationOptions"/></param>
        /// <param name="metrics">The <see cref="IRepository"/> used to manage <see cref="V1ApplicationMetrics"/></param>
        public V1RuntimemetricsDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, 
            IOptions<SynapseApplicationOptions> applicationOptions, IRepository<V1ApplicationMetrics> metrics) 
            : base(loggerFactory, mapper, mediator, integrationEventBus, applicationOptions)
        {
            this.Metrics = metrics;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1ApplicationMetrics"/>
        /// </summary>
        protected IRepository<V1ApplicationMetrics> Metrics { get; }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.TotalDefinitions++;
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.TotalInstances++;
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceStartedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.RunningInstances++;
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowInstanceExecutedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.RunningInstances--;
            metrics.ExecutedInstances++;
            switch (e.Status)
            {
                case V1WorkflowInstanceStatus.Completed:
                    metrics.CompletedInstances++;
                    break;
                case V1WorkflowInstanceStatus.Faulted:
                    metrics.FaultedInstances++;
                    break;
                case V1WorkflowInstanceStatus.Cancelled:
                    metrics.CancelledInstances++;
                    break;
            }
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.TotalActivities++;
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityStartedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.RunningActivities++;
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowActivityExecutedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var metrics = await this.GetOrCreateRuntimemetricsForAsync(e.CreatedAt, cancellationToken);
            metrics.RunningActivities--;
            metrics.ExecutedActivities++;
            switch (e.Status)
            {
                case V1WorkflowActivityStatus.Completed:
                    metrics.CompletedActivities++;
                    break;
                case V1WorkflowActivityStatus.Faulted:
                    metrics.FaultedActivities++;
                    break;
                case V1WorkflowActivityStatus.Cancelled:
                    metrics.CancelledActivities++;
                    break;
                case V1WorkflowActivityStatus.Skipped:
                    metrics.SkippedActivities++;
                    break;
            }
            await this.Metrics.UpdateAsync(metrics, cancellationToken);
            await this.Metrics.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets or creates the <see cref="V1ApplicationMetrics"/> for the specified date
        /// </summary>
        /// <param name="date">The date to get or create the <see cref="V1ApplicationMetrics"/> for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="V1ApplicationMetrics"/> for the specified date</returns>
        protected virtual async Task<V1ApplicationMetrics> GetOrCreateRuntimemetricsForAsync(DateTimeOffset date, CancellationToken cancellationToken)
        {
            var metrics = await this.Metrics.FindAsync(date.Date.ToString("yyyyMMdd"));
            if (metrics == null)
            {
                metrics = await this.Metrics.AddAsync(new(date.Date), cancellationToken);
                await this.Metrics.SaveChangesAsync(cancellationToken);
            }
            return metrics;
        }

    }

}
