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
    /// Represents the <see cref="ICommand"/> used to fault the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1FaultWorkflowInstanceCommand))]
    public class V1FaultWorkflowInstanceCommand
        : Command<Integration.Models.V1WorkflowInstance>
    {

        /// <summary>
        /// Initializes a new <see cref="V1FaultWorkflowInstanceCommand"/>
        /// </summary>
        protected V1FaultWorkflowInstanceCommand()
        {
            this.Id = null!;
            this.Error = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1FaultWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to fault</param>
        /// <param name="error">The <see cref="Error"/> that caused the <see cref="V1WorkflowInstance"/> to fault</param>
        public V1FaultWorkflowInstanceCommand(string id, Error error)
        {
            this.Id = id;
            this.Error = error;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to fault
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Error"/> that caused the <see cref="V1WorkflowInstance"/> to fault
        /// </summary>
        public virtual Neuroglia.Error Error { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1FaultWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1FaultWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1FaultWorkflowInstanceCommand, Integration.Models.V1WorkflowInstance>
    {

        /// <inheritdoc/>
        public V1FaultWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowInstance>> HandleAsync(V1FaultWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            var logs = await this.Mediator.ExecuteAndUnwrapAsync(new V1CollectWorkflowInstanceLogsCommand(workflowInstance), cancellationToken);
            workflowInstance.Fault(command.Error, logs);
            workflowInstance = await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowInstance>(workflowInstance));
        }

    }


}
