/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Synapse.Application.Configuration;
using Synapse.Infrastructure.Services;
using Synapse.Runtime.Docker.Configuration;
using Synapse.Runtime.Services;

namespace Synapse.Runtime
{

    /// <summary>
    /// Defines extensions for <see cref="ISynapseApplicationBuilder"/>s
    /// </summary>
    public static class ISynapseApplicationBuilderExtensions
    {

        /// <summary>
        /// Uses a Docker-base <see cref="IWorkflowRuntime"/>
        /// </summary>
        /// <param name="app">The <see cref="ISynapseApplicationBuilder"/> to configure</param>
        /// <returns>The configured <see cref="ISynapseApplicationBuilder"/></returns>
        public static ISynapseApplicationBuilder UseDockerRuntimeHost(this ISynapseApplicationBuilder app)
        {
            var runtimeHostOptions = new DockerRuntimeHostOptions();
            app.Configuration.Bind("docker", runtimeHostOptions);
            app.Services.AddSingleton<DockerRuntimeHost>();
            app.Services.AddSingleton<IWorkflowRuntime>(provider => provider.GetRequiredService<DockerRuntimeHost>());
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
