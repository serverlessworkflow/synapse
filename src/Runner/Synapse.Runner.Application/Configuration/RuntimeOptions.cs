using System;

namespace Synapse.Runner.Application.Configuration
{

    /// <summary>
    /// Represents the options used to configure the Synapse Runner runtime
    /// </summary>
    public class RuntimeOptions
    {

        /// <summary>
        /// Gets the default grace period, in seconds
        /// </summary>
        public const int DefaultGracePeriod = 30;

        /// <summary>
        /// Initializes a new <see cref="RuntimeOptions"/>
        /// </summary>
        public RuntimeOptions()
        {
            string raw = SynapseConstants.EnvironmentVariables.Runtime.GracePeriod.Value;
            if (!string.IsNullOrWhiteSpace(raw)
                && TimeSpan.TryParse(raw, out TimeSpan gracePeriod))
                this.GracePeriod = gracePeriod;
            else
                this.GracePeriod = TimeSpan.FromSeconds(DefaultGracePeriod);
        }

        /// <summary>
        /// Gets/sets the grace period, meaning the amount of time to substract from a runner's schedule, to make sure the application is up and running at the desired time
        /// </summary>
        public TimeSpan GracePeriod { get; set; }

        /// <summary>
        /// Gets/sets the options used to configure the Synapse Runner's runtime correlation feature
        /// </summary>
        public RuntimeCorrelationOptions Correlation { get; set; } = new RuntimeCorrelationOptions();

    }

}
