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
    /// Represents the <see cref="ICommand"/> used to schedule the execution of an existing <see cref="V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1ScheduleWorkflowInstanceCommand))]
    public class V1ScheduleWorkflowInstanceCommand
        : Command
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleWorkflowInstanceCommand"/>
        /// </summary>
        protected V1ScheduleWorkflowInstanceCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleWorkflowInstanceCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowInstance"/> to schedule</param>
        /// <param name="at">The date and time at which to schedule the <see cref="V1WorkflowInstance"/></param>
        public V1ScheduleWorkflowInstanceCommand(string id, DateTimeOffset at)
        {
            this.Id = id;
            this.At = at;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> to schedule
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the date and time at which to schedule the <see cref="V1WorkflowInstance"/>
        /// </summary>
        public virtual DateTimeOffset At { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ScheduleWorkflowInstanceCommand"/>s
    /// </summary>
    public class V1ScheduleWorkflowInstanceCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1ScheduleWorkflowInstanceCommand>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ScheduleWorkflowInstanceCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        /// <param name="runtimeHost">the current <see cref="IWorkflowRuntimeHost"/></param>
        public V1ScheduleWorkflowInstanceCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IRepository<V1WorkflowInstance> workflowInstances, IWorkflowRuntimeHost runtimeHost)
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
            this.RuntimeHost = runtimeHost;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntimeHost"/>
        /// </summary>
        protected IWorkflowRuntimeHost RuntimeHost { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult> HandleAsync(V1ScheduleWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            var runtimeId = await this.RuntimeHost.ScheduleAsync(workflowInstance, command.At, cancellationToken); //todo: something with runtime id
            workflowInstance.Scheduling();
            await this.WorkflowInstances.UpdateAsync(workflowInstance, cancellationToken);
            await this.WorkflowInstances.SaveChangesAsync(cancellationToken);
            return this.Ok();
        }

    }

}
