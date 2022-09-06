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

namespace Synapse.Application.Commands.WorkflowActivities
{
    /// <summary>
    /// Represents the <see cref="ICommand"/> used to mark a <see cref="V1WorkflowActivity"/> as compensated
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowActivities.V1MarkActivityAsCompensatedCommand))]
    public class V1MarkActivityAsCompensatedCommand
        : Command<Integration.Models.V1WorkflowActivity>
    {


        /// <summary>
        /// Initializes a new <see cref="V1MarkActivityAsCompensatedCommand"/>
        /// </summary>
        protected V1MarkActivityAsCompensatedCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1MarkActivityAsCompensatedCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> to mark as compensated</param>
        public V1MarkActivityAsCompensatedCommand(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowActivity"/> to mark as compensated
        /// </summary>
        public virtual string Id { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1MarkActivityAsCompensatedCommand"/>s
    /// </summary>
    public class V1MarkActivityAsCompensatedCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1MarkActivityAsCompensatedCommand, Integration.Models.V1WorkflowActivity>
    {

        /// <inheritdoc/>
        public V1MarkActivityAsCompensatedCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowActivity> activities)
            : base(loggerFactory, mediator, mapper)
        {
            this.Activities = activities;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        protected IRepository<V1WorkflowActivity> Activities { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowActivity>> HandleAsync(V1MarkActivityAsCompensatedCommand command, CancellationToken cancellationToken = default)
        {
            var activity = await this.Activities.FindAsync(command.Id, cancellationToken);
            if (activity == null)
                throw DomainException.NullReference(typeof(V1WorkflowActivity), command.Id);
            activity.MarkAsCompensated();
            activity = await this.Activities.UpdateAsync(activity, cancellationToken);
            await this.Activities.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowActivity>(activity));
        }

    }

}
