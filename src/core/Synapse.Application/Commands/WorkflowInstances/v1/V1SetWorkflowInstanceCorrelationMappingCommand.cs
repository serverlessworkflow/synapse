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
    /// Represents the <see cref="ICommand"/> used to set a correlation mapping for a <see cref="V1WorkflowActivity"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1SetWorkflowInstanceCorrelationMappingCommand))]
    public class V1SetWorkflowInstanceCorrelationMappingCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceCorrelationMappingCommand"/>
        /// </summary>
        protected V1SetWorkflowInstanceCorrelationMappingCommand()
        {
            this.Id = null!;
            this.Key = null!;
            this.Value = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceCorrelationMappingCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> to set the correlation mapping for</param>
        /// <param name="key">The key of the correlation mapping to set</param>
        /// <param name="value">The value of the correlation mapping to set</param>
        public V1SetWorkflowInstanceCorrelationMappingCommand(string id, string key, string value)
        {
            this.Id = id;
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowActivity"/> to cancel
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the key of the correlation mapping to set
        /// </summary>
        public virtual string Key { get; protected set; }

        /// <summary>
        /// Gets the value of the correlation mapping to set
        /// </summary>
        public virtual string Value { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SetWorkflowInstanceCorrelationMappingCommand"/>s
    /// </summary>
    public class V1SetWorkflowInstanceCorrelationMappingCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SetWorkflowInstanceCorrelationMappingCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceCorrelationMappingCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        public V1SetWorkflowInstanceCorrelationMappingCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1SetWorkflowInstanceCorrelationMappingCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            workflowInstance.SetCorrelationMapping(command.Key, command.Value);
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(workflowInstance));
        }

    }

}
