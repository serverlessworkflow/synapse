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

using Neuroglia.Serialization;

namespace Synapse.Application.Queries.WorkflowActivities
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to get the data of a <see cref="V1WorkflowActivity"/>'s parent state
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Queries.WorkflowActivities.V1GetActivityParentStateDataQuery))]
    public class V1GetActivityParentStateDataQuery
        : Query<Dynamic>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetActivityParentStateDataQuery"/>
        /// </summary>
        protected V1GetActivityParentStateDataQuery()
        {
            this.ActivityId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GetActivityParentStateDataQuery"/>
        /// </summary>
        /// <param name="activityId">The id of the activity to get the parent state data of belongs to</param>
        public V1GetActivityParentStateDataQuery(string activityId)
        {
            this.ActivityId = activityId;
        }

        /// <summary>
        /// Gets the id of the activity to get the parent state data of belongs to
        /// </summary>
        public virtual string ActivityId { get; protected set; }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetActivityParentStateDataQuery"/>
    /// </summary>
    public class V1GetActivityParentStateDataQueryHandler
       : QueryHandlerBase<Integration.Models.V1WorkflowActivity>,
        IQueryHandler<V1GetActivityParentStateDataQuery, Dynamic>
    {

        /// <inheritdoc/>
        public V1GetActivityParentStateDataQueryHandler(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, IRepository<Integration.Models.V1WorkflowActivity> repository)
            : base(loggerFactory, mediator, mapper, repository)
        {

        }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<Dynamic>> HandleAsync(V1GetActivityParentStateDataQuery query, CancellationToken cancellationToken = default)
        {
            var activity = await this.Repository.FindAsync(query.ActivityId, cancellationToken);
            if (activity == null)
                throw DomainException.NullReference(typeof(V1WorkflowActivity), query.ActivityId);
            while (activity != null)
            {
                if (activity.Type == V1WorkflowActivityType.State)
                    return this.Ok(activity.Input);
                activity = await this.Repository.FindAsync(activity.ParentId, cancellationToken);
            }
            return this.Ok();
        }

    }

}
