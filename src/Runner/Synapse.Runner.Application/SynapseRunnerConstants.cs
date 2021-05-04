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

            public static string GetHeader(string workflowId, string workflowVersion, string workflowInstance)
            {
                return SynapseConstants.Logging.Header + $@"
            
            SYNAPSE RUNNER {typeof(SynapseRunnerConstants).Assembly.GetName().Version}
        
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
