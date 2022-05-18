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

using Synapse.Domain.Events.V1WorkflowProcesses;

namespace Synapse.Application.Events.Domain
{

    /// <summary>
    /// Represents the service used to handle <see cref="V1WorkflowProcess"/>-related <see cref="IDomainEvent"/>s
    /// </summary>
    public class V1WorkflowProcessDomainEventHandler
        : DomainEventHandlerBase<V1WorkflowProcess, Integration.Models.V1WorkflowProcess, string>,
        INotificationHandler<V1WorkflowProcessStartedDomainEvent>,
        INotificationHandler<V1WorkflowProcessLogOutputDomainEvent>,
        INotificationHandler<V1WorkflowProcessExitedDomainEvent>
    {

        /// <inheritdoc/>
        public V1WorkflowProcessDomainEventHandler(ILoggerFactory loggerFactory, IMapper mapper, IMediator mediator, IIntegrationEventBus integrationEventBus, IOptions<SynapseApplicationOptions> synapseOptions, 
            IRepository<V1WorkflowProcess> aggregates, IRepository<Integration.Models.V1WorkflowProcess> projections) 
            : base(loggerFactory, mapper, mediator, integrationEventBus, synapseOptions, aggregates, projections)
        {

        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowProcessStartedDomainEvent e, CancellationToken cancellationToken = default)
        {
            await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            await this.PublishIntegrationEventAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowProcessLogOutputDomainEvent e, CancellationToken cancellationToken = default)
        {
            var process = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            process.Logs += e.Log + Environment.NewLine;
            await this.Projections.UpdateAsync(process, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task HandleAsync(V1WorkflowProcessExitedDomainEvent e, CancellationToken cancellationToken = default)
        {
            var process = await this.GetOrReconcileProjectionAsync(e.AggregateId, cancellationToken);
            process.ExitedAt = e.CreatedAt.UtcDateTime;
            process.ExitCode = e.ExitCode;
            await this.Projections.UpdateAsync(process, cancellationToken);
            await this.Projections.SaveChangesAsync(cancellationToken);
        }

    }

}