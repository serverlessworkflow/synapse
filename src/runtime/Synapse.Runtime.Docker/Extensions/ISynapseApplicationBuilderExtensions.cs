using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Synapse.Application.Configuration;
using Synapse.Application.Services;
using Synapse.Runtime.Docker.Configuration;
using Synapse.Runtime.Services;

namespace Synapse.Runtime.Docker
{

    /// <summary>
    /// Defines extensions for <see cref="ISynapseApplicationBuilder"/>s
    /// </summary>
    public static class ISynapseApplicationBuilderExtensions
    {

        /// <summary>
        /// Uses a Docker-base <see cref="IWorkflowRuntimeHost"/>
        /// </summary>
        /// <param name="app">The <see cref="ISynapseApplicationBuilder"/> to configure</param>
        /// <returns>The configured <see cref="ISynapseApplicationBuilder"/></returns>
        public static ISynapseApplicationBuilder UseDockerRuntimeHost(this ISynapseApplicationBuilder app)
        {
            var runtimeHostOptions = new DockerRuntimeHostOptions();
            app.Configuration.Bind("docker", runtimeHostOptions);
            app.Services.AddSingleton<DockerRuntimeHost>();
            app.Services.AddSingleton<IWorkflowRuntimeHost>(provider => provider.GetRequiredService<DockerRuntimeHost>());
            app.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DockerRuntimeHost>());
            app.Services.AddSingleton<IDockerClient>(provider =>
            {
                return new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
            });
            app.Services.TryAddSingleton(Options.Create(runtimeHostOptions));
            return app;
        }

    }

}
