﻿/*
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

namespace Synapse.Application.Commands.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to skip the execution of a <see cref="Domain.Models.V1WorkflowActivity"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowActivities.V1SkipWorkflowActivityCommand))]
    public class V1SkipWorkflowActivityCommand
        : Command<Integration.Models.V1WorkflowActivity>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SkipWorkflowActivityCommand"/>
        /// </summary>
        protected V1SkipWorkflowActivityCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SkipWorkflowActivityCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="Domain.Models.V1WorkflowActivity"/> to fault</param>
        public V1SkipWorkflowActivityCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="Domain.Models.V1WorkflowActivity"/> to cancel
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SkipWorkflowActivityCommand"/>s
    /// </summary>
    public class V1SkipWorkflowActivityCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SkipWorkflowActivityCommand, Integration.Models.V1WorkflowActivity>
    {

        /// <inheritdoc/>
        public V1SkipWorkflowActivityCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Domain.Models.V1WorkflowActivity> activities)
            : base(loggerFactory, mediator, mapper)
        {
            this.Activities = activities;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="Domain.Models.V1WorkflowActivity"/> instances
        /// </summary>
        protected IRepository<Domain.Models.V1WorkflowActivity> Activities { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowActivity>> HandleAsync(V1SkipWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            var activity = await this.Activities.FindAsync(command.Id, cancellationToken);
            if (activity == null)
                throw DomainException.NullReference(typeof(Domain.Models.V1WorkflowActivity), command.Id);
            activity.Skip();
            activity = await this.Activities.UpdateAsync(activity, cancellationToken);
            await this.Activities.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowActivity>(activity));
        }

    }

}
