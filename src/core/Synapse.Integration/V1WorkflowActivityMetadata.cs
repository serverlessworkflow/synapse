namespace Synapse
{

    /// <summary>
    /// Exposes constants about <see cref="V1WorkflowActivity"/>-related metadata
    /// </summary>
    public static class V1WorkflowActivityMetadata
    {

        /// <summary>
        /// Gets the name of the 'action' metadata, used to store the name of the action the activity belongs to
        /// </summary>
        public const string Action = "action";
        /// <summary>
        /// Gets the name of the 'branch' metadata, used to store the name of the branch the activity belongs to
        /// </summary>
        public const string Branch = "branch";
        /// <summary>
        /// Gets the name of the 'case' metadata, used to store the name of the switch case the activity belongs to
        /// </summary>
        public const string Case = "case";
        /// <summary>
        /// Gets the name of the 'compensation-source' metadata, used to reference the activity that is being compensated
        /// </summary>
        public const string CompensationSource= "compensation-source";
        /// <summary>
        /// Gets the name of the 'event' metadata, used to store the name of the action the activity belongs to
        /// </summary>
        public const string Event = "event";
        /// <summary>
        /// Gets the name of the 'iteration' metadata, used to store the index of the iteration to process
        /// </summary>
        public const string Iteration = "iteration";
        /// <summary>
        /// Gets the name of the 'state' metadata, used to store the name of the state the activity belongs to
        /// </summary>
        public const string State = "state";
        /// <summary>
        /// Gets the name of the 'subflow' metadata, used to store the id of the subflow's instance
        /// </summary>
        public const string Subflow = "subflow";
        /// <summary>
        /// Gets the name of the 'next-state' metadata, used to define the state to explicitely transition to
        /// </summary>
        public const string NextState = "next-state";
        /// <summary>
        /// Gets the name of the 'trigger' metadata, used to store the name of the event trigger the activity belongs to
        /// </summary>
        public const string Trigger = "trigger";

    }

}
