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
    /// Represents the <see cref="ICommand"/> used to mark the execution of a <see cref="Domain.Models.V1WorkflowInstance"/> as started
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1SetWorkflowInstanceStartedCommand))]
    public class V1SetWorkflowInstanceStartedCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceStartedCommand"/>
        /// </summary>
        protected V1SetWorkflowInstanceStartedCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowInstanceStartedCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="Domain.Models.V1WorkflowInstance"/> to mark as started</param>
        public V1SetWorkflowInstanceStartedCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="Domain.Models.V1WorkflowInstance"/> to mark as started
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SetWorkflowInstanceStartedCommand"/>s
    /// </summary>
    public class V1SetWorkflowInstanceStartedCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SetWorkflowInstanceStartedCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <inheritdoc/>
        public V1SetWorkflowInstanceStartedCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Domain.Models.V1WorkflowInstance> workflowInstances) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Domain.Models.V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<Domain.Models.V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1SetWorkflowInstanceStartedCommand command, CancellationToken cancellationToken = default)
        {
            var instance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (instance == null)
                throw DomainException.NullReference(typeof(Domain.Models.V1WorkflowInstance), command.Id);
            instance.MarkAsRunning();
            instance = await this.WorkflowInstances.UpdateAsync(instance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(instance));
        }

    }

}
