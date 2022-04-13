namespace Synapse.Worker.Configuration
{
    /// <summary>
    /// Represents the object used to configure a Synapse API client
    /// </summary>
    public class ApiClientConfiguration
    {

        /// <summary>
        /// Gets/sets the scheme of the API to connect to
        /// </summary>
        public virtual string? Scheme { get; set; }

        /// <summary>
        /// Gets/sets the port number of the API to connect to
        /// </summary>
        public virtual int? Port { get; set; }

        /// <summary>
        /// Gets the default HTTP-based <see cref="ApiClientConfiguration"/>
        /// </summary>
        public static ApiClientConfiguration HttpDefault
        {
            get
            {
                return new() { Scheme = "http", Port = 42286 };
            }
        }

        /// <summary>
        /// Gets the default GRPC-based <see cref="ApiClientConfiguration"/>
        /// </summary>
        public static ApiClientConfiguration GrpcDefault
        {
            get
            {
                return new() { Scheme = "http", Port = 41387 };
            }
        }

    }

}
