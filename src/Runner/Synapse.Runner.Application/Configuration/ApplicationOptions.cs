namespace Synapse.Runner.Application.Configuration
{

    /// <summary>
    /// Represents the options used to configure the Synapse Runner application
    /// </summary>
    public class ApplicationOptions
    {

        /// <summary>
        /// Gets/sets the options used to configure the Synapse Runner runtime
        /// </summary>
        public RuntimeOptions Runtime { get; set; } = new RuntimeOptions();

    }

}
