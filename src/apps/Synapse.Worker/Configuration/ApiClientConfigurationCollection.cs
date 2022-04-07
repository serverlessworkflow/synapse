namespace Synapse.Worker.Executor.Configuration
{

    /// <summary>
    /// Represents the object used to configure the Synapse APIs clients used to by the application
    /// </summary>
    public class ApiClientConfigurationCollection
    {

        /// <summary>
        /// Gets the default Synapse server's host name
        /// </summary>
        public static string DefaultHostName
        {
            get
            {
                var env = EnvironmentVariables.Api.HostName.Value;
                if (!string.IsNullOrWhiteSpace(env))
                    return env;
                return "synapse";
            }
        }

        /// <summary>
        /// Gets/sets the Synapse server's host name. Defaults to <see cref="DefaultHostName"/>
        /// </summary>
        public virtual string HostName { get; set; } = DefaultHostName;

        /// <summary>
        /// Gets/sets an object used to configure Synapse http-based APIs
        /// </summary>
        public virtual ApiClientConfiguration Http { get; set; } = ApiClientConfiguration.HttpDefault;

        /// <summary>
        /// Gets/sets an object used to configure Synapse GRPC-based APIs
        /// </summary>
        public virtual ApiClientConfiguration Grpc { get; set; } = ApiClientConfiguration.GrpcDefault;

    }

}
