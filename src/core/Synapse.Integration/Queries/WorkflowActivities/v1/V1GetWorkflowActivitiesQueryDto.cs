namespace Synapse.Integration.Queries.WorkflowActivities
{

    /// <summary>
    /// Describes the query used to find specific workflow activities
    /// </summary>
    [DataContract]
    public class V1GetWorkflowActivitiesQueryDto
        : DataTransferObject
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowActivitiesQueryDto"/>
        /// </summary>
        public V1GetWorkflowActivitiesQueryDto()
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="V1GetWorkflowActivitiesQueryDto"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance to get the workflow activity instances of</param>
        /// <param name="includeNonOperative">A boolean indicating whether or not to include non-operative activities</param>
        /// <param name="parentId">The id of the workflow activity to get the child activities of</param>
        public V1GetWorkflowActivitiesQueryDto(string workflowInstanceId, bool includeNonOperative = false, string parentId = null)
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
