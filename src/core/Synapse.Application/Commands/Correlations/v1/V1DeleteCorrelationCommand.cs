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
    /// Represents the <see cref="ICommand"/> used to delete an existing <see cref="V1Correlation"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Correlations.V1DeleteCorrelationCommand))]
    public class V1DeleteCorrelationCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelationCommand"/>
        /// </summary>
        protected V1DeleteCorrelationCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelationCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Correlation"/> to delete</param>
        public V1DeleteCorrelationCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Correlation"/> to delete
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1DeleteCorrelationCommand"/>s
    /// </summary>
    public class V1DeleteCorrelationCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1DeleteCorrelationCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1DeleteCorrelationCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="correlations">The <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s</param>
        public V1DeleteCorrelationCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Correlation> correlations)
            : base(loggerFactory, mediator, mapper)
        {
            this.Correlations = correlations;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s
        /// </summary>
        protected IRepository<V1Correlation> Correlations { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1DeleteCorrelationCommand command, CancellationToken cancellationToken = default)
        {
            var correlation = await this.Correlations.FindAsync(command.Id, cancellationToken);
            if (correlation == null)
                throw DomainException.NullReference(typeof(V1Correlation), command.Id);
            correlation.Delete();
            await this.Correlations.UpdateAsync(correlation, cancellationToken);
            await this.Correlations.RemoveAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            return this.Ok();
        }
    }

}
