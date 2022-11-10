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

using Synapse.Domain.Events.Correlations;

namespace Synapse.Application.Events.Domain
{

    /// <summary>
    /// Represents the service used to handle <see cref="V1Correlation"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1CorrelationDomainEventHandler
        : DomainEventHandlerBase<V1Correlation, Integration.Models.V1Correlation, string>,
        INotificationHandler<V1CorrelationCreatedDomainEvent>,
        INotificationHandler<V1ContextAddedToCorrelationDomainEvent>,
        INotificationHandler<V1EventCorrelatedDomainEvent>,
        INotificationHandler<V1CorrelatedEventReleasedDomainEvent>,
        INotificationHandler<V1CorrelationContextReleasedDomainEvent>,
        INotificationHandler<V1CorrelationDeletedDomainEvent>
    {

        /// <inheritdoc/>
        public V1CorrelationDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus,
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1Correlation> aggregates, IRepository<Integration.Models.V1Correlation> projections)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1CorrelationCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1ContextAddedToCorrelationDomainEvent e, CancellationToken cancellationToken = default)
        {
            var correlation = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            if (correlation.Contexts == null) correlation.Contexts = new List<Integration.Models.V1CorrelationContext>();
            correlation.Contexts.Add(this.Mapper.Map<Integration.Models.V1CorrelationContext>(e.Context));
            await this.Projections.UpdateAsync(correlation, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1EventCorrelatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var correlation = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            var context = correlation.Contexts.FirstOrDefault(c => c.Id == e.ContextId);
            if (context == null) return;
            if (context.PendingEvents == null) context.PendingEvents = new List<Integration.Models.V1Event>();
            if (context.PendingEvents.Any(evt => evt.Id == e.Event.Id)) return;
            context.PendingEvents.Add(this.Mapper.Map< Integration.Models.V1Event>(e.Event));
            await this.Projections.UpdateAsync(correlation, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1CorrelatedEventReleasedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var correlation = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            if (correlation.Contexts == null) correlation.Contexts = new List<Integration.Models.V1CorrelationContext>();
            var context = correlation.Contexts.FirstOrDefault(c => c.Id == e.ContextId);
            if (context == null) return;
            var evt = context.PendingEvents?.FirstOrDefault(x => x.Id.Equals(e.EventId, StringComparison.InvariantCultureIgnoreCase));
            if (evt == null) return;
            context.PendingEvents!.Remove(evt);
            await this.Projections.UpdateAsync(correlation, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1CorrelationContextReleasedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var correlation = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            if (correlation.Contexts == null) correlation.Contexts = new List<Integration.Models.V1CorrelationContext>();
            var context = correlation.Contexts.FirstOrDefault(c => c.Id == e.ContextId);
            if (context == null) return;
            correlation.Contexts.Remove(context);
            await this.Projections.UpdateAsync(correlation, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1CorrelationDeletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var correlation = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.Projections.RemoveAsync(correlation, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

    }

}
