using CloudNative.CloudEvents;
using ServerlessWorkflow.Sdk;
using System;

namespace Synapse.Runner.Application.Configuration
{
    /// <summary>
    /// Represents the options used to configure the Synapse Runner's runtime <see cref="CloudEvent"/>'s correlation feature
    /// </summary>
    public class RuntimeCorrelationOptions
    {

        /// <summary>
        /// Gets the default timeout, in seconds
        /// </summary>
        public const int DefaultTimeout = 30;

        /// <summary>
        /// Initializes a new <see cref="RuntimeCorrelationOptions"/>
        /// </summary>
        public RuntimeCorrelationOptions()
        {
            string raw;
            raw = SynapseConstants.EnvironmentVariables.Runtime.Correlation.Mode.Value;
            if (!string.IsNullOrWhiteSpace(raw))
                this.Mode = EnumHelper.Parse<RuntimeCorrelationMode>(raw);
            if (this.Mode == RuntimeCorrelationMode.Passive)
                return;
            raw = SynapseConstants.EnvironmentVariables.Runtime.Correlation.MaxDuration.Value;
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
