namespace Synapse.Operator.Application.Configuration
{

    /// <summary>
    /// Represents the options used to configure the Synapse Operator
    /// </summary>
    public class ApplicationOptions
    {

        /// <summary>
        /// Gets the options used to configure a Synapse Runner
        /// </summary>
        public RunnerOptions Runner { get; set; } = new RunnerOptions();

    }

}
