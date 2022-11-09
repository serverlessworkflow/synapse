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
    /// Represents the <see cref="ICommand"/> used to delete a <see cref="V1Correlation"/>'s <see cref="V1CorrelationContext"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Correlations.V1DeleteCorrelationContextCommand))]
    public class V1DeleteCorrelationContextCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelationCommand"/>
        /// </summary>
        protected V1DeleteCorrelationContextCommand() { }

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelationCommand"/>
        /// </summary>
        /// <param name="correlationId">The id of the <see cref="V1Correlation"/> the <see cref="V1CorrelationContext"/> to delete belongs to</param>
        /// <param name="contextId">The id of the <see cref="V1CorrelationContext"/> to delete</param>
        public V1DeleteCorrelationContextCommand(string correlationId, string contextId)
        {
            this.CorrelationId = correlationId;
            this.ContextId = contextId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Correlation"/> the <see cref="V1CorrelationContext"/> to delete belongs to
        /// </summary>
        public virtual string CorrelationId { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="V1CorrelationContext"/> to delete
        /// </summary>
        public virtual string ContextId { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1DeleteCorrelationContextCommand"/>s
    /// </summary>
    public class V1DeleteCorrelationContextCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1DeleteCorrelationContextCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelationCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="correlations">The <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s</param>
        public V1DeleteCorrelationContextCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Correlation, string> correlations) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Correlations = correlations;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s
        /// </summary>
        protected IRepository<V1Correlation, string> Correlations { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1DeleteCorrelationContextCommand command, CancellationToken cancellationToken = default)
        {
            var correlation = await this.Correlations.FindAsync(command.CorrelationId, cancellationToken);
            if (correlation == null) throw DomainException.NullReference(typeof(V1Correlation), command.CorrelationId);
            var context = correlation.Contexts?.FirstOrDefault(c => c.Id.Equals(command.ContextId, StringComparison.InvariantCultureIgnoreCase));
            if(context == null) throw DomainException.NullReference(typeof(V1CorrelationContext), command.ContextId);
            correlation.ReleaseContext(context);
            await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            return this.Ok();
        }

    }

}
