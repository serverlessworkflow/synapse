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

using Synapse.Domain.Events.EventDefinitionCollections;

namespace Synapse.Application.Events.Domain
{
    /// <summary>
    /// Represents the service used to handle <see cref="V1EventDefinitionCollection"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1EventDefinitionCollectionDomainEventHandler
        : DomainEventHandlerBase<V1EventDefinitionCollection, Integration.Models.V1EventDefinitionCollection, string>,
        INotificationHandler<V1EventDefinitionCollectionCreatedDomainEvent>,
        INotificationHandler<V1EventDefinitionCollectionDeletedDomainEvent>
    {

        /// <inheritdoc/>
        public V1EventDefinitionCollectionDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus,
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1EventDefinitionCollection> aggregates, IRepository<Integration.Models.V1EventDefinitionCollection> projections)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1EventDefinitionCollectionCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1EventDefinitionCollectionDeletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var collection = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.Projections.RemoveAsync(collection, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

    }

}