namespace Synapse.Runtime.Executor.Configuration
{

    /// <summary>
    /// Represents the object used to configure the application
    /// </summary>
    public class ApplicationOptions
    {

        /// <summary>
        /// Gets/sets the options used to configure the runtime's event correlation feature
        /// </summary>
        public CorrelationOptions Correlation { get; set; } = new();

    }

}
