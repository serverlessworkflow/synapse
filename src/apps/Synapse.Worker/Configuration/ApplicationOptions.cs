namespace Synapse.Worker.Configuration
{

    /// <summary>
    /// Represents the object used to configure the application
    /// </summary>
    public class ApplicationOptions
    {

        /// <summary>
        /// Initializes a new <see cref="ApplicationOptions"/>
        /// </summary>
        public ApplicationOptions()
        {
            var env = EnvironmentVariables.SkipCertificateValidation.Value;
            if (!string.IsNullOrWhiteSpace(env)
                && bool.TryParse(env, out var skipCertificateValidation))
                this.SkipCertificateValidation = skipCertificateValidation;
        }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not to skip certificate validation when performing http requests
        /// </summary>
        public virtual bool SkipCertificateValidation { get; set; }

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
