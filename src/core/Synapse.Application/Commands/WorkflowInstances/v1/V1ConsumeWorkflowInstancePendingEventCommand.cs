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

using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Application.Commands.WorkflowInstances
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to consume a pending event of an existing <see cref="Domain.Models.V1WorkflowInstance"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowInstances.V1ConsumeWorkflowInstancePendingEventCommand))]
    public class V1ConsumeWorkflowInstancePendingEventCommand
        : Command<Integration.Models.V1Event?>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ConsumeWorkflowInstancePendingEventCommand"/>
        /// </summary>
        protected V1ConsumeWorkflowInstancePendingEventCommand()
        {
            this.Id = null!;
            this.EventDefinition = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1ConsumeWorkflowInstancePendingEventCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="Domain.Models.V1WorkflowInstance"/> to consume a pending event of</param>
        /// <param name="eventDefinition">The <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that describes the event to consume</param>
        public V1ConsumeWorkflowInstancePendingEventCommand(string id, EventDefinition eventDefinition)
        {
            this.Id = id;
            this.EventDefinition = eventDefinition;
        }

        /// <summary>
        /// Gets the id of the <see cref="Domain.Models.V1WorkflowInstance"/> to consume a pending event of
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.EventDefinition"/> that describes the event to consume
        /// </summary>
        public virtual EventDefinition EventDefinition { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1ConsumeWorkflowInstancePendingEventCommand"/>s
    /// </summary>
    public class V1ConsumeWorkflowInstancePendingEventCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1ConsumeWorkflowInstancePendingEventCommand, Integration.Models.V1Event?>
    {

        /// <summary>
        /// Initializes a new <see cref="V1ConsumeWorkflowInstancePendingEventCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowInstances">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s</param>
        public V1ConsumeWorkflowInstancePendingEventCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowInstance> workflowInstances) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1Event?>> HandleAsync(V1ConsumeWorkflowInstancePendingEventCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.Id, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.Id);
            var e = workflowInstance.CorrelationContext.PendingEvents
                .FirstOrDefault(e => e.Matches(command.EventDefinition));
            return this.Ok(e == null ? null : this.Mapper.Map<Integration.Models.V1Event>(e));
        }

    }

}
