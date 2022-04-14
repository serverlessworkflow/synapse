namespace Synapse.Integration.Queries.WorkflowActivities
{
    /// <summary>
    /// Represents the query used to get the data of a workflow activity's parent state
    /// </summary>
    [DataContract]
    public class V1GetActivityParentStateDataQuery
        : DataTransferObject
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetActivityParentStateDataQuery"/>
        /// </summary>
        public V1GetActivityParentStateDataQuery()
        {
            this.WorkflowInstanceId = null!;
            this.ActivityId = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1GetActivityParentStateDataQuery"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the workflow instance the activity to get the parent state data of belongs to</param>
        /// <param name="activityId">The id of the activity to get the parent state data of belongs to</param>
        public V1GetActivityParentStateDataQuery(string workflowInstanceId, string activityId)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.ActivityId = activityId;
        }

        /// <summary>
        /// Gets the id of the workflow instance the activity to get the parent state data of belongs to
        /// </summary>
        [DataMember(Order = 1)]
        public virtual string WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets the id of the activity to get the parent state data of belongs to
        /// </summary>
        [DataMember(Order = 2)]
        public virtual string ActivityId { get; set; }

    }

}
