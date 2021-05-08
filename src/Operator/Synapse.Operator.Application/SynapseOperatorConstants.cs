namespace Synapse.Operator.Application
{

    /// <summary>
    /// Exposes constants about the Synapse Operator
    /// </summary>
    public static class SynapseOperatorConstants
    {

        /// <summary>
        /// Exposes constants about Synapse Operator's logging
        /// </summary>
        public static class Logging
        {

            /// <summary>
            /// Gets the Synapse Operator log header
            /// </summary>
            /// <returns>The Synapse Operator log header</returns>
            public static string GetHeader()
            {
                return SynapseConstants.Logging.Header + $@"           
SYNAPSE OPERATOR {typeof(SynapseOperatorConstants).Assembly.GetName().Version}

";
            }

        }

    }

}
