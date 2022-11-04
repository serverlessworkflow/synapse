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

namespace Synapse.Application.Commands.Schedules
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1Schedule"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.Schedules.V1CreateScheduleCommand))]
    public class V1CreateScheduleCommand
        : Command<Integration.Models.V1Schedule>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateScheduleCommand"/>
        /// </summary>
        protected V1CreateScheduleCommand() { }

        /// <summary>
        /// Initializes a new <see cref="V1CreateScheduleCommand"/>
        /// </summary>
        /// <param name="type">The type of the <see cref="V1Schedule"/> to create</param>
        /// <param name="definition">The definition of the <see cref="V1Schedule"/> to create</param>
        /// <param name="workflowId">The id of the <see cref="V1Workflow"/> to schedule</param>
        public V1CreateScheduleCommand(V1ScheduleType type, ScheduleDefinition definition, string workflowId)
        {
            this.Type = type;
            this.Definition = definition;
            this.WorkflowId = workflowId;
        }

        /// <summary>
        /// Gets the type of the <see cref="V1Schedule"/> to create
        /// </summary>
        public virtual V1ScheduleType Type { get; protected set; }

        /// <summary>
        /// Gets the definition of the <see cref="V1Schedule"/> to create
        /// </summary>
        public virtual ScheduleDefinition Definition { get; protected set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="V1Workflow"/> to schedule
        /// </summary>
        public virtual string WorkflowId { get; protected set; } = null!;

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateScheduleCommand"/>s
    /// </summary>
    public class V1CreateScheduleCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateScheduleCommand, Integration.Models.V1Schedule>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateScheduleCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflows">The <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s</param>
        /// <param name="schedules">The <see cref="IRepository"/> used to manage <see cref="V1Schedule"/>s</param>
        public V1CreateScheduleCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1Workflow> workflows, IRepository<V1Schedule> schedules) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Workflows = workflows;
            this.Schedules = schedules;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Workflow"/>s
        /// </summary>
        protected IRepository<V1Workflow> Workflows { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1Schedule"/>s
        /// </summary>
        protected IRepository<V1Schedule> Schedules { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1Schedule>> HandleAsync(V1CreateScheduleCommand command, CancellationToken cancellationToken = default)
        {
            var workflowId = (await this.Mediator.ExecuteAndUnwrapAsync(Queries.Workflows.V1GetWorkflowByIdQuery.Parse(command.WorkflowId), cancellationToken))?.Id;
            if(string.IsNullOrWhiteSpace(workflowId)) throw DomainException.NullReference(typeof(V1Workflow), command.WorkflowId);
            var workflow = await this.Workflows.FindAsync(workflowId, cancellationToken);
            if (workflow == null) throw DomainException.NullReference(typeof(V1Workflow), workflowId);
            var schedule = await this.Schedules.AddAsync(new(command.Type, command.Definition, workflow), cancellationToken);
            await this.Schedules.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1Schedule>(schedule));
        }

    }

}
