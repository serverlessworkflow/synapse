namespace Synapse.Application.Configuration
{

    /// <summary>
    /// Represents the options used to configure the Synapse application
    /// </summary>
    public class SynapseApplicationOptions
    {

        /// <summary>
        /// Gets/sets the options used to configure the application's cloud eventss
        /// </summary>
        public virtual CloudEventOptions CloudEvents { get; set; } = new();

    }

}
