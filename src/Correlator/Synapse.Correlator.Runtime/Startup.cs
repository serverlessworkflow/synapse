using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Synapse.Runner.Application.Configuration;

namespace Synapse.Correlator.Runtime
{

    public class Startup
    {

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        protected IConfiguration Configuration { get; }

        protected IHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSynapseBroker(this.Configuration, this.Environment);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseCloudEvents();
        }

    }

}
