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

namespace Synapse.Application.Commands.Correlations
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to delete a correlated <see cref="V1Event"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Correlations.V1DeleteCorrelatedEventCommand))]
    public class V1DeleteCorrelatedEventCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelatedEventCommand"/>
        /// </summary>
        protected V1DeleteCorrelatedEventCommand() { }

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelatedEventCommand"/>
        /// </summary>
        /// <param name="correlationId">The id of the <see cref="V1Correlation"/> the <see cref="V1Event"/> to delete belongs to</param>
        /// <param name="contextId">The id of the <see cref="V1CorrelationContext"/> the <see cref="V1Event"/> to delete belongs to</param>
        /// <param name="eventId">The id of the <see cref="V1Event"/> to delete</param>
        public V1DeleteCorrelatedEventCommand(string correlationId, string contextId,string eventId)
        {
            this.CorrelationId = correlationId;
            this.ContextId = contextId;
            this.EventId = eventId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Correlation"/> the <see cref="V1Event"/> to delete belongs to
        /// </summary>
        public virtual string CorrelationId { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="V1CorrelationContext"/> the <see cref="V1Event"/> to delete belongs to
        /// </summary>
        public virtual string ContextId { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="V1Event"/> to delete
        /// </summary>
        public virtual string EventId { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1DeleteCorrelatedEventCommand"/>s
    /// </summary>
    public class V1DeleteCorrelatedEventCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1DeleteCorrelatedEventCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelatedEventCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="correlations">The <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s</param>
        public V1DeleteCorrelatedEventCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Correlation, string> correlations)
            : base(loggerFactory, mediator, mapper)
        {
            this.Correlations = correlations;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s
        /// </summary>
        protected IRepository<V1Correlation, string> Correlations { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1DeleteCorrelatedEventCommand command, CancellationToken cancellationToken = default)
        {
            var correlation = await this.Correlations.FindAsync(command.CorrelationId, cancellationToken);
            if (correlation == null) throw DomainException.NullReference(typeof(V1Correlation), command.CorrelationId);
            var context = correlation.Contexts?.FirstOrDefault(c => c.Id.Equals(command.ContextId, StringComparison.InvariantCultureIgnoreCase));
            if (context == null) throw DomainException.NullReference(typeof(V1CorrelationContext), command.ContextId);
            var evt = context.PendingEvents?.FirstOrDefault(e => e.Id.Equals(command.EventId, StringComparison.InvariantCultureIgnoreCase));
            if (evt == null) throw DomainException.NullReference(typeof(V1Event), command.EventId);
            correlation.ReleaseEvent(context, evt);
            await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            return this.Ok();
        }

    }

}
