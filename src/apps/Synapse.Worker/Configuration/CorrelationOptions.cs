namespace Synapse.Worker.Configuration
{
    /// <summary>
    /// Represents the object used to configure the runtime's event correlation feature
    /// </summary>
    public class CorrelationOptions
    {

        /// <summary>
        /// Gets the default timeout, in seconds
        /// </summary>
        public const int DefaultTimeout = 30;

        /// <summary>
        /// Initializes a new <see cref="CorrelationOptions"/>
        /// </summary>
        public CorrelationOptions()
        {
            string raw;
            raw = EnvironmentVariables.Runtime.Correlation.Mode.Value;
            if (!string.IsNullOrWhiteSpace(raw))
                this.Mode = EnumHelper.Parse<RuntimeCorrelationMode>(raw);
            if (this.Mode == RuntimeCorrelationMode.Passive)
                return;
            raw = EnvironmentVariables.Runtime.Correlation.MaxActiveDuration.Value;
            if (!string.IsNullOrWhiteSpace(raw)
                && TimeSpan.TryParse(raw, out TimeSpan timeout))
                this.Timeout = timeout;
            else
                this.Timeout = TimeSpan.FromSeconds(DefaultTimeout);
        }

        /// <summary>
        /// Gets/sets the application's correlation mode
        /// </summary>
        public RuntimeCorrelationMode Mode { get; set; }

        /// <summary>
        /// Gets/sets the amount of time after which the Synapse Runner falls back to the <see cref="RuntimeCorrelationMode.Passive"/>, if <see cref="Mode"/> is set to <see cref="RuntimeCorrelationMode.Dual"/>
        /// </summary>
        public TimeSpan? Timeout { get; set; }

    }

}
