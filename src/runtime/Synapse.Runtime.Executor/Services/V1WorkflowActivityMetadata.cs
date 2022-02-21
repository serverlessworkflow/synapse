namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Exposes constants about <see cref="V1WorkflowActivity"/>-related metadata
    /// </summary>
    public static class V1WorkflowActivityMetadata
    {

        /// <summary>
        /// Gets the name of the 'state' metadata, used to store the name of the state the activity belongs to
        /// </summary>
        public const string State = "state";
        /// <summary>
        /// Gets the name of the 'case' metadata, used to store the name of the switch case the activity belongs to
        /// </summary>
        public const string Case = "case";

    }

}
