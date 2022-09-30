﻿/*
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

using Synapse.Domain.Authentications.AuthenticationDefinitionCollections;

namespace Synapse.Application.Events.Domain
{
    /// <summary>
    /// Represents the service used to handle <see cref="V1AuthenticationDefinitionCollection"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1AuthenticationDefinitionCollectionDomainEventHandler
        : DomainEventHandlerBase<V1AuthenticationDefinitionCollection, Integration.Models.V1AuthenticationDefinitionCollection, string>,
        INotificationHandler<V1AuthenticationDefinitionCollectionCreatedDomainEvent>,
        INotificationHandler<V1AuthenticationDefinitionCollectionDeletedDomainEvent>
    {

        /// <inheritdoc/>
        public V1AuthenticationDefinitionCollectionDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus,
            IOptions<SynapseApplicationOptions> synapseOptions, IRepository<V1AuthenticationDefinitionCollection> aggregates, IRepository<Integration.Models.V1AuthenticationDefinitionCollection> projections)
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1AuthenticationDefinitionCollectionCreatedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1AuthenticationDefinitionCollectionDeletedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var collection = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.Projections.RemoveAsync(collection, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

    }

}