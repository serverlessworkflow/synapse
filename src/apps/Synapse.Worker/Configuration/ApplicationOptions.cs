namespace Synapse.Worker.Executor.Configuration
{

    /// <summary>
    /// Represents the object used to configure the application
    /// </summary>
    public class ApplicationOptions
    {

        /// <summary>
        /// Gets/sets the object used to configure the Synapse APIs clients used to by the application
        /// </summary>
        public virtual ApiClientConfigurationCollection Api { get; set; } = new();

        /// <summary>
        /// Gets/sets the options used to configure the runtime's event correlation feature
        /// </summary>
        public virtual CorrelationOptions Correlation { get; set; } = new();

    }

}
