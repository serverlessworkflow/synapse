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

using Synapse.Application.Commands.WorkflowInstances;
using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.Commands.Correlations
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1Correlation"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Correlations.V1CreateCorrelationCommand))]
    public class V1CreateCorrelationCommand
        : Command<Integration.Models.V1Correlation>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateCorrelationCommand"/>
        /// </summary>
        protected V1CreateCorrelationCommand()
        {
            this.Conditions = null!;
            this.Outcome = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="activationType">The activation type of the <see cref="V1Correlation"/> to create</param>
        /// <param name="lifetime">The lifetime of the <see cref="V1Correlation"/> to create</param>
        /// <param name="conditionType">The type of <see cref="V1CorrelationCondition"/> evaluation the <see cref="V1Correlation"/> should use</param>
        /// <param name="conditions">An <see cref="IEnumerable{T}"/> containing all <see cref="V1CorrelationCondition"/>s the <see cref="V1Correlation"/> to create is made out of</param>
        /// <param name="outcome">The <see cref="V1CorrelationOutcome"/> of the <see cref="V1Correlation"/> to create</param>
        /// <param name="context">The initial <see cref="V1CorrelationContext"/> of the <see cref="V1Correlation"/> to create</param>
        public V1CreateCorrelationCommand(V1CorrelationActivationType activationType, V1CorrelationLifetime lifetime, V1CorrelationConditionType conditionType, 
            IEnumerable<V1CorrelationCondition> conditions, V1CorrelationOutcome outcome, V1CorrelationContext? context)
        {
            this.ActivationType = activationType;
            this.Lifetime = lifetime;
            this.ConditionType = conditionType;
            this.Conditions = conditions.ToList();
            this.Outcome = outcome;
            this.Context = context;
        }

        /// <summary>
        /// Gets the activation type of the <see cref="V1Correlation"/> to create
        /// </summary>
        public virtual V1CorrelationActivationType ActivationType { get; protected set; }

        /// <summary>
        /// Gets the lifetime of the <see cref="V1Correlation"/> to create
        /// </summary>
        [Required]
        public virtual V1CorrelationLifetime Lifetime { get; protected set; }

        /// <summary>
        /// Gets the type of <see cref="V1CorrelationCondition"/> evaluation the <see cref="V1Correlation"/> should use
        /// </summary>
        [Required]
        public virtual V1CorrelationConditionType ConditionType { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing all <see cref="V1CorrelationCondition"/>s the <see cref="V1Correlation"/> to create is made out of
        /// </summary>
        [MinLength(1)]
        public virtual ICollection<V1CorrelationCondition> Conditions { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1CorrelationOutcome"/> of the <see cref="V1Correlation"/> to create
        /// </summary>
        [Required]
        public virtual V1CorrelationOutcome Outcome { get; protected set; }

        /// <summary>
        /// Gets the initial <see cref="V1CorrelationContext"/> of the <see cref="V1Correlation"/> to create
        /// </summary>
        public virtual V1CorrelationContext? Context { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateCorrelationCommand"/>s
    /// </summary>
    public class V1CreateCorrelationCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateCorrelationCommand, Integration.Models.V1Correlation>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateCorrelationCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="correlations">The <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s</param>
        public V1CreateCorrelationCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Correlation> correlations) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Correlations = correlations;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Correlation"/>s
        /// </summary>
        protected IRepository<V1Correlation> Correlations { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1Correlation>> HandleAsync(V1CreateCorrelationCommand command, CancellationToken cancellationToken = default)
        {
            var correlation = new V1Correlation(command.ActivationType, command.Lifetime, command.ConditionType, command.Conditions, command.Outcome, command.Context);
            correlation = await this.Correlations.AddAsync(correlation, cancellationToken);
            await this.Correlations.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1Correlation>(correlation));
        }

    }

}
