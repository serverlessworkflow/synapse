// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Synapse.Runtime.Services;
using static Synapse.SynapseDefaults.Resources;
using System.Net;

namespace Synapse.Runtime.Docker.Services;

/// <summary>
/// Represents the Docker implementation of the <see cref="IWorkflowRuntime"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="environment">The current <see cref="IHostEnvironment"/></param>
/// <param name="runner">The service used to access the current <see cref="RunnerConfiguration"/></param>
public class DockerRuntime(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IHostEnvironment environment, IOptions<RunnerConfiguration> runner)
    : WorkflowRuntimeBase(loggerFactory)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the current <see cref="IHostEnvironment"/>
    /// </summary>
    protected IHostEnvironment Environment { get; } = environment;

    /// <summary>
    /// Gets the current <see cref="RunnerConfiguration"/>
    /// </summary>
    protected RunnerConfiguration Runner => runner.Value;

    /// <summary>
    /// Gets the service used to interact with the Docker API
    /// </summary>
    protected IDockerClient? Docker { get; set; }

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all managed runner processes
    /// </summary>
    protected ConcurrentDictionary<string, DockerWorkflowProcess> Processes { get; } = new();

    /// <summary>
    /// Initializes the <see cref="DockerRuntime"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.Runner.Runtime.Docker == null) throw new NullReferenceException($"Failed to initialize the Docker Runtime because the operator is not configured to use Docker as a runtime");
        var dockerConfiguration = new DockerClientConfiguration(this.Runner.Runtime.Docker.Api.Endpoint);
        this.Docker = dockerConfiguration.CreateClient(string.IsNullOrWhiteSpace(this.Runner.Runtime.Docker.Api.Version) ? null : System.Version.Parse(this.Runner.Runtime.Docker.Api.Version!));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, ServiceAccount serviceAccount, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(workflowInstance);
        ArgumentNullException.ThrowIfNull(serviceAccount);
        try
        {
            this.Logger.LogDebug("Creating a new Docker container for workflow instance '{workflowInstance}'...", workflowInstance.GetQualifiedName());
            if (this.Docker == null) await this.InitializeAsync(cancellationToken).ConfigureAwait(false); 
            var container = this.Runner.Runtime.Docker!.ContainerTemplate.Clone()!;
            try
            {
                await this.Docker!.Images.InspectImageAsync(container.Image, cancellationToken).ConfigureAwait(false);
            }
            catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var downloadProgress = new Progress<JSONMessage>();
                var imageComponents = container.Image.Split(':');
                var imageName = imageComponents[0];
                var imageTag = imageComponents.Length > 1 ? imageComponents[1] : null;
                await this.Docker!.Images.CreateImageAsync(new() { FromImage = imageName, Tag = imageTag }, new(), downloadProgress, cancellationToken).ConfigureAwait(false);
            }
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Namespace, workflowInstance.GetNamespace()!);
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.Name, $"{workflowInstance.GetName()}-{Guid.NewGuid().ToString("N")[..12].ToLowerInvariant()}");
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Api.Uri, this.Runner.Api.Uri.OriginalString);
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.ContainerPlatform, this.Runner.ContainerPlatform);
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Runner.LifecycleEvents, (this.Runner.PublishLifecycleEvents ?? true).ToString());
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Secrets.Directory, this.Runner.Runtime.Docker.Secrets.MountPath);
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.ServiceAccount.Name, serviceAccount.GetQualifiedName());
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.ServiceAccount.Key, serviceAccount.Spec.Key);
            container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.Workflow.Instance, workflowInstance.GetQualifiedName());
            container.SetEnvironmentVariable("DOCKER_HOST", "unix:///var/run/docker.sock");
            container.User = "root";
            if (this.Runner.Certificates?.Validate == false) container.SetEnvironmentVariable(SynapseDefaults.EnvironmentVariables.SkipCertificateValidation, "true");
            var hostConfig = new HostConfig()
            {
                Mounts = []
            };
            if (!Directory.Exists(this.Runner.Runtime.Docker.Secrets.Directory)) Directory.CreateDirectory(this.Runner.Runtime.Docker.Secrets.Directory);
            hostConfig.Mounts.Add(new()
            {
                Type = "bind",
                Source = this.Runner.Runtime.Docker.Secrets.Directory,
                Target = this.Runner.Runtime.Docker.Secrets.MountPath
            });
            hostConfig.Mounts.Add(new()
            {
                Type = "bind",
                Source = "/var/run/docker.sock",
                Target = "/var/run/docker.sock"
            });
            hostConfig.ExtraHosts =
            [
                "host.docker.internal:host-gateway"
            ];
            var parameters = new CreateContainerParameters(container)
            {
                Name = $"{workflowInstance.GetQualifiedName()}-{Guid.NewGuid().ToString("N")[..12].ToLowerInvariant()}",
                HostConfig = hostConfig
            };
            var result = await this.Docker!.Containers.CreateContainerAsync(parameters, cancellationToken).ConfigureAwait(false);
            if (this.Environment.RunsInDocker()) await this.Docker.Networks.ConnectNetworkAsync(this.Runner.Runtime.Docker.Network, new NetworkConnectParameters() { Container = result.ID }, cancellationToken);
            if (result.Warnings.Count > 0) this.Logger.LogWarning("Warnings have been raised during container creation: {warnings}", string.Join(System.Environment.NewLine, result.Warnings));
            var process = ActivatorUtilities.CreateInstance<DockerWorkflowProcess>(this.ServiceProvider, this.Docker!, result.ID);
            this.Processes.TryAdd(process.Id, process);
            this.Logger.LogDebug("A new container with id '{id}' has been successfully created to run workflow instance '{workflowInstance}'", process.Id, workflowInstance.GetQualifiedName());
            return process;
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while creating a new Docker process for workflow instance '{workflowInstance}': {ex}", workflowInstance.GetQualifiedName(), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    protected override ValueTask DisposeAsync(bool disposing)
    {
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        this.Docker?.Dispose();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        this.Docker?.Dispose();
        base.Dispose(disposing);
    }

}
