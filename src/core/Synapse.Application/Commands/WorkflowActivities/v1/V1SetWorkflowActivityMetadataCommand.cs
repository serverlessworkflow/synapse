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
    /// Represents the <see cref="ICommand"/> used to set the metadata of a <see cref="V1WorkflowActivity"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Commands.WorkflowActivities.V1SetWorkflowActivityMetadataCommand))]
    public class V1SetWorkflowActivityMetadataCommand
        : Command<Integration.Models.V1WorkflowActivity>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowActivityMetadataCommand"/>
        /// </summary>
        protected V1SetWorkflowActivityMetadataCommand()
        {
            this.Id = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowActivityMetadataCommand"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1WorkflowActivity"/> to update the metadata of</param>
        /// <param name="metadata">An <see cref="IDictionary{TKey, TValue}"/> containing the <see cref="V1WorkflowActivity"/>'s updated metadata</param>
        public V1SetWorkflowActivityMetadataCommand(string id, IDictionary<string, string>? metadata)
        {
            this.Id = id;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1WorkflowActivity"/> to update the metadata of
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IDictionary{TKey, TValue}"/> containing the <see cref="V1WorkflowActivity"/>'s updated metadata
        /// </summary>
        public virtual IDictionary<string, string>? Metadata { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1SetWorkflowActivityMetadataCommand"/>s
    /// </summary>
    public class V1SetWorkflowActivityMetadataCommandHandler
        : CommandHandlerBase,
        ICommandHandler<V1SetWorkflowActivityMetadataCommand, Integration.Models.V1WorkflowActivity>
    {

        /// <summary>
        /// Initializes a new <see cref="V1SetWorkflowActivityMetadataCommandHandler"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="activities">The <see cref="IRepository"/> used to manage <see cref="V1WorkflowActivity"/> instances</param>
        public V1SetWorkflowActivityMetadataCommandHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<V1WorkflowActivity> activities)
            : base(loggerFactory, mediator, mapper)
        {
            this.Activities = activities;
        }

        /// <summary>
        /// Gets the <see cref="IRepository"/> used to manage <see cref="V1WorkflowActivity"/> instances
        /// </summary>
        protected IRepository<V1WorkflowActivity> Activities { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Integration.Models.V1WorkflowActivity>> HandleAsync(V1SetWorkflowActivityMetadataCommand command, CancellationToken cancellationToken = default)
        {
            var activity = await this.Activities.FindAsync(command.Id, cancellationToken);
            if (activity == null)
                throw DomainException.NullReference(typeof(V1WorkflowActivity), command.Id);
            activity.SetMetadata(command.Metadata);
            activity = await this.Activities.UpdateAsync(activity, cancellationToken);
            await this.Activities.SaveChangesAsync(cancellationToken);
            return this.Ok(this.Mapper.Map<Integration.Models.V1WorkflowActivity>(activity));
        }

    }
}
