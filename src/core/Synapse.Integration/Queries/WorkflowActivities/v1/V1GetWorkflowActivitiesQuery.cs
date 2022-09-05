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

namespace Synapse.Integration.Queries.WorkflowActivities
{

    /// <summary>
    /// Describes the query used to find specific workflow activities
    /// </summary>
    [DataContract]
    public class V1GetWorkflowActivitiesQuery
        : DataTransferObject
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowActivitiesQuery"/>
        /// </summary>
        public V1GetWorkflowActivitiesQuery()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowActivitiesQuery"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to get the workflow activity instances of</param>
        /// <param name="includeNonOperative">A boolean indicating whether or not to include non-operative activities</param>
        /// <param name="parentId">The id of the workflow activity to get the child activities of</param>
        public V1GetWorkflowActivitiesQuery(string workflowInstanceId, bool includeNonOperative = false, string parentId = null)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.IncludeNonOperative = includeNonOperative;
            this.ParentId = parentId;
        }

        /// <summary>
        /// Gets the id of the workflow instance to get the workflow activity instances of
        /// </summary>
        [DataMember(Order = 1)]
        public virtual string WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether or not to include non-operative activities
        /// </summary>
        [DataMember(Order = 2)]
        public virtual bool IncludeNonOperative { get; set; }

        /// <summary>
        /// Gets the id of the workflow activity to get the child activities of
        /// </summary>
        [DataMember(Order = 3)]
        public virtual string ParentId { get; set; }

    }

}
