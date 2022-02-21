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
    /// Represents the <see cref="ICommand"/> used to suspend a <see cref="V1WorkflowActivity"/>
    /// </summary>
    [DataTransferObjectType(typeof(V1SuspendWorkflowActivityCommandDto))]
    public class V1SuspendWorkflowActivityCommand
        : Command<V1WorkflowActivityDto>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SuspendWorkflowActivityCommand"/>
        /// </summary>
        protected V1SuspendWorkflowActivityCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SuspendWorkflowActivityCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1InitializeWorkflowActivityCommand"/> to suspend</param>
        public V1SuspendWorkflowActivityCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowActivity"/> to suspend
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SuspendWorkflowActivityCommand"/>s
    /// </summary>
    public class V1SuspendWorkflowActivityCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SuspendWorkflowActivityCommand, V1WorkflowActivityDto>
    {

        /// <inheritdoc/>
        public V1SuspendWorkflowActivityCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowActivity> activities)
            : base(loggerFactory, mediator, mapper)
        {
            this.Activities = activities;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        protected IRepository<V1WorkflowActivity> Activities { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1WorkflowActivityDto>> HandleAsync(V1SuspendWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            var activity = await this.Activities.FindAsync(command.Id, cancellationToken);
            if (activity == null)
                throw DomainException.NullReference(typeof(V1WorkflowActivity), command.Id);
            activity.Suspend();
            activity = await this.Activities.UpdateAsync(activity, cancellationToken);
            await this.Activities.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<V1WorkflowActivityDto>(activity));
        }

    }

}
