namespace Synapse.Runtime.Services
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
        /// Gets the name of the 'state' metadata, used to store the name of the state the activity belongs to
        /// </summary>
        public const string State = "state";
        /// <summary>
        /// Gets the name of the 'trigger' metadata, used to store the name of the event trigger the activity belongs to
        /// </summary>
        public const string Trigger = "trigger";

    }

}
