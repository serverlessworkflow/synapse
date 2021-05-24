namespace Synapse.Correlator.Application
{

    /// <summary>
    /// Exposes constants about the Synapse Broker
    /// </summary>
    public static class SynapseCorrelatorConstants
    {

        /// <summary>
        /// Exposes constants about Synapse Broker's logging
        /// </summary>
        public static class Logging
        {

            /// <summary>
            /// Gets the Synapse Broker log header
            /// </summary>
            /// <returns>The Synapse Broker log header</returns>
            public static string GetHeader()
            {
                return SynapseConstants.Logging.Header + $@"           
SYNAPSE BROKER {SynapseConstants.Version}        

            ";
            }

        }

    }

}
