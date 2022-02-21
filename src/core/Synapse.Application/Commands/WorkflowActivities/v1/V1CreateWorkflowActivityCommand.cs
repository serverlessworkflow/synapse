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
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Models;

namespace Synapse.Application.Commands.WorkflowActivities
{

    /// <summary>
    /// Represents the <see cref="ICommand"/> used to create a new <see cref="V1WorkflowActivity"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1CreateWorkflowActivityCommandDto))]
    public class V1CreateWorkflowActivityCommand
        : Command<V1WorkflowActivityDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowActivityCommand"/>
        /// </summary>
        protected V1CreateWorkflowActivityCommand()
        {
            this.WorkflowInstanceId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CreateWorkflowActivityCommand"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowActivity"/> to create belongs to</param>
        /// <param name="type">The type of <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="input">The input data of the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="metadata">The metadata of the <see cref="V1WorkflowActivity"/> to create</param>
        /// <param name="parentId">The id the parent of the <see cref="V1WorkflowActivity"/> to create</param>
        public V1CreateWorkflowActivityCommand(string workflowInstanceId, V1WorkflowActivityType type, object? input, IDictionary<string, string>? metadata, string? parentId)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.Type = type;
            this.Input = input;
            this.Metadata = metadata;
            this.ParentId = parentId;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowInstance"/> the <see cref="V1WorkflowActivity"/> to create belongs to
        /// </summary>
        public virtual string WorkflowInstanceId { get; protected set; }

        /// <summary>
        /// Gets the type of <see cref="V1WorkflowActivity"/> to create
        /// </summary>
        public virtual V1WorkflowActivityType Type { get; protected set; }

        /// <summary>
        /// Gets the input data of the <see cref="V1WorkflowActivity"/> to create
        /// </summary>
        public virtual object? Input { get; protected set; }

        /// <summary>
        /// Gets the metadata of the <see cref="V1WorkflowActivity"/> to create
        /// </summary>
        public virtual IDictionary<string, string>? Metadata { get; protected set; }

        /// <summary>
        /// Gets the id the parent of the <see cref="V1WorkflowActivity"/> to create
        /// </summary>
        public virtual string? ParentId { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1CreateWorkflowActivityCommand"/>s
    /// </summary>
    public class V1CreateWorkflowActivityCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1CreateWorkflowActivityCommand, V1WorkflowActivityDto>
    {

        /// <inheritdoc/>
        public V1CreateWorkflowActivityCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IRepository<V1WorkflowInstance> workflowInstances, IRepository<V1WorkflowActivity> workflowActivities) 
            : base(loggerFactory, mediator, mapper)
        {
            this.WorkflowInstances = workflowInstances;
            this.WorkflowActivities = workflowActivities;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowInstance"/>s
        /// </summary>
        protected virtual IRepository<V1WorkflowInstance> WorkflowInstances { get; }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        protected virtual IRepository<V1WorkflowActivity> WorkflowActivities { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowActivityDto>> HandleAsync(V1CreateWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            var workflowInstance = await this.WorkflowInstances.FindAsync(command.WorkflowInstanceId, cancellationToken);
            if (workflowInstance == null)
                throw DomainException.NullReference(typeof(V1WorkflowInstance), command.WorkflowInstanceId);
            var parent = null as V1WorkflowActivity;
            if (!string.IsNullOrWhiteSpace(command.ParentId))
            {
                parent = await this.WorkflowActivities.FindAsync(command.ParentId, cancellationToken);
                if (parent == null)
                    throw DomainException.NullReference(typeof(V1WorkflowActivity), command.ParentId);
            }
            var activity = await this.WorkflowActivities.AddAsync(new(workflowInstance, command.Type, command.Input, command.Metadata, parent), cancellationToken);
            await this.WorkflowActivities.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowActivityDto>(activity));
        }

    }

}
