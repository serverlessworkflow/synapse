namespace Synapse.Runner.Application
{

    /// <summary>
    /// Exposes constants about the Synapse Runner
    /// </summary>
    public static class SynapseRunnerConstants
    {

        /// <summary>
        /// Exposes constants about Synapse Runner's logging
        /// </summary>
        public static class Logging
        {

            /// <summary>
            /// Gets the Synapse Runner log header
            /// </summary>
            /// <returns>The Synapse Runner log header</returns>
            public static string GetHeader(string workflowId, string workflowVersion, string workflowInstance)
            {
                return SynapseConstants.Logging.Header + $@"           
SYNAPSE RUNNER {SynapseConstants.Version}
        
------------------------------------------------------------------------------------------------------------------------------------------------------------------------
WORKFLOW
------------------------------------------------------------------------------------------------------------------------------------------------------------------------
DEFINITION ID:         {workflowId}
DEFINITION VERSION:    {workflowVersion}
INSTANCE ID:           {workflowInstance}

            ";
            }

        }

    }

}
