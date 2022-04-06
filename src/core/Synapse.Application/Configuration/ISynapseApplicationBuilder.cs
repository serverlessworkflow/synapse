using AutoMapper;

namespace Synapse.Application.Configuration
{

    /// <summary>
    /// Defines the fundamentals of a service used to build a Synapse application
    /// </summary>
    public interface ISynapseApplicationBuilder
    {

        /// <summary>
        /// Gets the application's <see cref="IConfiguration"/>
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the application's <see cref="IServiceCollection"/>
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures the <see cref="ISynapseApplicationBuilder"/> to use the specified <see cref="Profile"/>
        /// </summary>
        /// <typeparam name="TProfile">The type of the <see cref="Profile"/> to use</typeparam>
        /// <returns>The configured <see cref="ISynapseApplicationBuilder"/></returns>
        ISynapseApplicationBuilder AddMappingProfile<TProfile>()
            where TProfile : Profile;

    }

}
