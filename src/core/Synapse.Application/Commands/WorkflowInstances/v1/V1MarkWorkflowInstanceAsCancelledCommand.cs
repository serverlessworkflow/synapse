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
    /// Represents the <see cref="ICommand"/> used to mark the execution of a <see cref="V1WorkflowInstance"/> as cancelled
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1MarkWorkflowInstanceAsCancelledCommand))]
    public class V1MarkWorkflowInstanceAsCancelledCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1MarkWorkflowInstanceAsCancelledCommand"/>
        /// </summary>
        protected V1MarkWorkflowInstanceAsCancelledCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1MarkWorkflowInstanceAsCancelledCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to mark as cancelled</param>
        public V1MarkWorkflowInstanceAsCancelledCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to mark as cancelled
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1MarkWorkflowInstanceAsCancelledCommand"/>s
    /// </summary>
    public class V1MarkWorkflowInstanceAsCancelledCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1MarkWorkflowInstanceAsCancelledCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <inheritdoc/>
        public V1MarkWorkflowInstanceAsCancelledCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1MarkWorkflowInstanceAsCancelledCommand command, CancellationToken cancellationToken = default)
        {
            var instance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (instance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            instance.MarkAsCancelled();
            instance = await this.WorkflowInstances.UpdateAsync(instance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(instance));
        }

    }

}
