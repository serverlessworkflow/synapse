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

namespace Synapse.Application.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to attempt correlating a <see cref="V1Event"/> to an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1TryCorrelateWorkflowInstanceCommand))]
    public class V1TryCorrelateWorkflowInstanceCommand
        : Command<bool>
    {

        /// <summary>
        /// Initializes a new <see cref="V1TryCorrelateWorkflowInstanceCommand"/>
        /// </summary>
        protected V1TryCorrelateWorkflowInstanceCommand()
        {
            this.WorkflowInstanceId = null!;
            this.Event = null!;
            this.MappingKeys = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1TryCorrelateWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> to correlate</param>
        /// <param name="e">The <see cref="V1Event"/> to correlate</param>
        /// <param name="mappingKeys">An <see cref="IEnumerable{T}"/> containing the mapping keys to use to correlate the <see cref="V1WorkflowInstance"/></param>
        public V1TryCorrelateWorkflowInstanceCommand(string workflowInstanceId, V1Event e, IEnumerable<string> mappingKeys)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.Event = e;
            this.MappingKeys = mappingKeys;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to correlate
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1Event"/> to correlate
        /// </summary>
        public virtual V1Event Event { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the mapping keys to use to correlate the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual IEnumerable<string> MappingKeys { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1TryCorrelateWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1TryCorrelateWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1TryCorrelateWorkflowInstanceCommand, bool>
    {

        /// <inheritdoc/>
        public V1TryCorrelateWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<bool>> HandleAsync(V1TryCorrelateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.WorkflowInstanceId, cancellationToken);
            if(workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.WorkflowInstanceId);
            if (!workflowInstance.CorrelationContext.CorrelatesTo(command.Event))
                return this.Ok(false);
            workflowInstance.CorrelationContext.Correlate(command.Event, command.MappingKeys);
            await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(true);
        }

    }
}
