using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Synapse.Runner.Application.Configuration;

namespace Synapse.Runner.Runtime
{

    /// <summary>
    /// Represents the object used to configure the application's startup
    /// </summary>
    public class Startup
    {

        /// <summary>
        /// Initializes a new <see cref="Startup"/>
        /// </summary>
        /// <param name="configuration">The current <see cref="IConfiguration"/></param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        /// <summary>
        /// Gets the current <see cref="IConfiguration"/>
        /// </summary>
        protected IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the current <see cref="IHostEnvironment"/>
        /// </summary>
        protected IHostEnvironment Environment { get; }

        /// <summary>
        /// Configures the application's services
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSynapseRunner(this.Configuration, this.Environment);
        }

        /// <summary>
        /// Configures the application's pipeline
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to configure</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        public void Configure(IApplicationBuilder app, IOptions<ApplicationOptions> options)
        {
            if (this.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            if(options.Value.Runtime.Correlation.Mode == RuntimeCorrelationMode.Active
                || options.Value.Runtime.Correlation.Mode == RuntimeCorrelationMode.Dual)
                app.UseCloudEvents();
        }

    }

}
