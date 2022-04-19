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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Synapse.Application.Services;
using Synapse.Domain.Models;
using Synapse.Runtime.Docker;
using Synapse.Runtime.Docker.Configuration;
using System.Diagnostics;
using System.Text;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the Docker implementation of the <see cref="IWorkflowRuntimeHost"/>
    /// </summary>
    public class DockerRuntimeHost
        : WorkflowRuntimeHostBase
    {

        /// <summary>
        /// Initializes a new <see cref="DockerRuntimeHost"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <param name="options">The service used to access the current <see cref="DockerRuntimeHostOptions"/></param>
        /// <param name="docker">The service used to interact with the Docker API</param>
        public DockerRuntimeHost(ILoggerFactory loggerFactory, IHostEnvironment environment, IOptions<DockerRuntimeHostOptions> options, IDockerClient docker)
            : base(loggerFactory)
        {
            this.Environment = environment;
            this.Options = options.Value;
            this.Docker = docker;
   
        }

        /// <summary>
        /// Gets the current <see cref="IHostEnvironment"/>
        /// </summary>
        protected IHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the current <see cref="DockerRuntimeHostOptions"/>
        /// </summary>
        protected DockerRuntimeHostOptions Options { get; }

        /// <summary>
        /// Gets the service used to interact with the Docker API
        /// </summary>
        protected IDockerClient Docker { get; }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var containerId = System.Environment.MachineName;
            var response = null as NetworkResponse;
            try
            {
                response = await this.Docker.Networks.InspectNetworkAsync(this.Options.Network, stoppingToken);
            }
            catch(DockerNetworkNotFoundException)
            {
                await this.Docker.Networks.CreateNetworkAsync(new() { Name = this.Options.Network }, stoppingToken);
            }
            finally
            {
                if (this.Environment.RunsInDocker()
                    && (response == null ? true : !response!.Containers.ContainsKey(containerId)))
                    await this.Docker.Networks.ConnectNetworkAsync(this.Options.Network, new NetworkConnectParameters() { Container = containerId }, stoppingToken);
            }
        }

        /// <inheritdoc/>
        public override async Task<string> StartAsync(V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            await this.PullRuntimeExecutorImageAsync(cancellationToken);
            var containerConfig = this.Options.Runtime.Container;
            containerConfig.AddOrUpdateEnvironmentVariable(EnvironmentVariables.Api.HostName.Name, EnvironmentVariables.Api.HostName.Value!); //todo: instead, fetch values from options
            containerConfig.AddOrUpdateEnvironmentVariable(EnvironmentVariables.Runtime.WorkflowInstanceId.Name, workflowInstance.Id.ToString()); //todo: instead, fetch values from options
            var name = workflowInstance.Id.Replace(":", "-");
            var hostConfig = new HostConfig()
            {
                Mounts = new List<Mount>()
            };
            if (!string.IsNullOrWhiteSpace(this.Options.Secrets.Directory))
            {
                hostConfig.Mounts.Add(new()
                {
                    Type = "bind",
                    Source = this.Options.Secrets.Directory,
                    Target = "/run/secrets"
                });
            }
            var createContainerParameters = new CreateContainerParameters(containerConfig)
            {
                Name = name,
                HostConfig = hostConfig
            };
            var createContainerResult = await this.Docker.Containers.CreateContainerAsync(createContainerParameters, cancellationToken);
            if (this.Environment.RunsInDocker())
                await this.Docker.Networks.ConnectNetworkAsync(this.Options.Network, new NetworkConnectParameters() { Container = createContainerResult.ID }, cancellationToken);
            foreach (var warning in createContainerResult.Warnings)
            {
                this.Logger.LogWarning(warning);
            }
            var startContainerParameters = new ContainerStartParameters();
            await this.Docker.Containers.StartContainerAsync(createContainerResult.ID, startContainerParameters, cancellationToken);
            return createContainerResult.ID;
        }

        /// <summary>
        /// Pulls the configured Synapse runner image
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task PullRuntimeExecutorImageAsync(CancellationToken cancellationToken)
        {
            if (this.Options.Runtime.ImagePullPolicy == ImagePullPolicy.Always)
            {
                await this.Docker.Images.CreateImageAsync(new() { FromImage = this.Options.Runtime.Container.Image, Repo = this.Options.Runtime.ImageRepository }, new(), new Progress<JSONMessage>(), cancellationToken);
            }
            else
            {
                try
                {
                    var result = await this.Docker.Images.InspectImageAsync(this.Options.Runtime.Container.Image, cancellationToken);
                }
                catch (DockerImageNotFoundException)
                {
                    await this.Docker.Images.CreateImageAsync(new() { FromImage = this.Options.Runtime.Container.Image, Repo = this.Options.Runtime.ImageRepository }, new(), new Progress<JSONMessage>(), cancellationToken);
                }
            }
        }

    }

}