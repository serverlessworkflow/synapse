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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk.IO;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Api.Application.Configuration;
using Synapse.Resources;
using System.Diagnostics;

namespace Synapse.Api.Application.Services;

/// <summary>
/// Defines the fundamentals of a service used to initialize the Synapse workflow database
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="yamlSerializer">The service used to serialize/deserialize data to/from YAML</param>
/// <param name="workflowDefinitionReader">The service used to read <see cref="WorkflowDefinition"/>s</param>
/// <param name="options">The service used to access the current <see cref="ApiServerOptions"/></param>
public class DatabaseProvisioner(IServiceProvider serviceProvider, ILogger<DatabaseProvisioner> logger, IJsonSerializer jsonSerializer, IYamlSerializer yamlSerializer, IWorkflowDefinitionReader workflowDefinitionReader, IOptions<ApiServerOptions> options)
    : IHostedService
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer { get; } = yamlSerializer;

    /// <summary>
    /// Gets the service used to read <see cref="WorkflowDefinition"/>s
    /// </summary>
    protected IWorkflowDefinitionReader WorkflowDefinitionReader { get; } = workflowDefinitionReader; 

    /// <summary>
    /// Gets the current <see cref="ApiServerOptions"/>
    /// </summary>
    protected ApiServerOptions Options { get; } = options.Value;

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = this.ServiceProvider.CreateScope();
        var resources = scope.ServiceProvider.GetRequiredService<IResourceRepository>();
        var stopwatch = new Stopwatch();
        if (this.Options.Seeding.Reset)
        {
            this.Logger.LogInformation("Starting resetting database...");
            stopwatch.Start();
            await foreach (var correlation in resources.GetAllAsync<Correlation>(cancellationToken: cancellationToken).ConfigureAwait(false)) await resources.RemoveAsync<Correlation>(correlation.GetName(), correlation.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            await foreach (var correlator in resources.GetAllAsync<Correlator>(cancellationToken: cancellationToken).ConfigureAwait(false)) await resources.RemoveAsync<Correlator>(correlator.GetName(), correlator.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            await foreach (var ns in resources.GetAllAsync<Namespace>(cancellationToken: cancellationToken).Where(ns => ns.GetName() != Namespace.DefaultNamespaceName)) await resources.RemoveAsync<Namespace>(ns.GetName(), ns.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            await foreach (var @operator in resources.GetAllAsync<Operator>(cancellationToken: cancellationToken).ConfigureAwait(false)) await resources.RemoveAsync<Operator>(@operator.GetName(), @operator.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            await foreach (var serviceAccount in resources.GetAllAsync<ServiceAccount>(cancellationToken: cancellationToken).ConfigureAwait(false)) await resources.RemoveAsync<ServiceAccount>(serviceAccount.GetName(), serviceAccount.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            await foreach (var workflow in resources.GetAllAsync<Workflow>(cancellationToken: cancellationToken).ConfigureAwait(false)) await resources.RemoveAsync<Workflow>(workflow.GetName(), workflow.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            await foreach (var workflowInstance in resources.GetAllAsync<WorkflowInstance>(cancellationToken: cancellationToken).ConfigureAwait(false)) await resources.RemoveAsync<WorkflowInstance>(workflowInstance.GetName(), workflowInstance.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();
            this.Logger.LogInformation("Database reset completed in {ms} milliseconds", stopwatch.Elapsed.TotalMilliseconds);
        }
        var directory = new DirectoryInfo(this.Options.Seeding.Directory);
        if (!directory.Exists)
        {
            this.Logger.LogWarning("The directory '{directory}' does not exist or cannot be found. Skipping static resource import", directory.FullName);
            return;
        }
        await this.ProvisionNamespacesAsync(resources, cancellationToken).ConfigureAwait(false);
        await this.ProvisionWorkflowsAsync(resources, cancellationToken).ConfigureAwait(false);
        await this.ProvisionFunctionsAsync(resources, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Provisions namespaces from statis resource files
    /// </summary>
    /// <param name="resources">The <see cref="IResourceRepository"/> used to manage <see cref="IResource"/>s</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ProvisionNamespacesAsync(IResourceRepository resources, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        var directory = new DirectoryInfo(Path.Combine(this.Options.Seeding.Directory, "namespaces"));
        if (!directory.Exists) return;
        this.Logger.LogInformation("Starting importing namespaces from directory '{directory}'...", directory.FullName);
        var files = directory.GetFiles(this.Options.Seeding.FilePattern, SearchOption.AllDirectories).Where(f => f.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));
        if (!files.Any())
        {
            this.Logger.LogWarning("No namespace static resource files matching search pattern '{pattern}' found in directory '{directory}'. Skipping import.", this.Options.Seeding.FilePattern, directory.FullName);
            return;
        }
        stopwatch.Restart();
        var count = 0;
        foreach (var file in files)
        {
            try
            {
                var extension = file.FullName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                var serializer = extension?.ToLowerInvariant() switch
                {
                    "json" => (ITextSerializer)this.JsonSerializer,
                    "yml" or "yaml" => this.YamlSerializer,
                    _ => throw new NotSupportedException($"The specified extension '{extension}' is not supported for static resource files")
                };
                using var stream = file.OpenRead();
                using var streamReader = new StreamReader(stream);
                var text = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                var ns = serializer.Deserialize<Namespace>(text)!;
                await resources.AddAsync(ns, false, cancellationToken).ConfigureAwait(false);
                this.Logger.LogInformation("Successfully imported namespace '{namespace}' from file '{file}'", $"{ns.Metadata.Name}", file.FullName);
                count++;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("An error occurred while reading a namespace from file '{file}': {ex}", file.FullName, ex);
                continue;
            }
        }
        stopwatch.Stop();
        this.Logger.LogInformation("Completed importing {count} namespaces in {ms} milliseconds", count, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Provisions workflows from statis resource files
    /// </summary>
    /// <param name="resources">The <see cref="IResourceRepository"/> used to manage <see cref="IResource"/>s</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ProvisionWorkflowsAsync(IResourceRepository resources, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        var directory = new DirectoryInfo(Path.Combine(this.Options.Seeding.Directory, "workflows"));
        if (!directory.Exists) return;
        this.Logger.LogInformation("Starting importing workflows from directory '{directory}'...", directory.FullName);
        var files = directory.GetFiles(this.Options.Seeding.FilePattern, SearchOption.AllDirectories).Where(f => f.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));
        if (!files.Any())
        {
            this.Logger.LogWarning("No workflow static resource files matching search pattern '{pattern}' found in directory '{directory}'. Skipping import.", this.Options.Seeding.FilePattern, directory.FullName);
            return;
        }
        stopwatch.Restart();
        var count = 0;
        foreach (var file in files)
        {
            try
            {
                using var stream = file.OpenRead();
                var workflowDefinition = await this.WorkflowDefinitionReader.ReadAsync(stream, new() { BaseDirectory = file.Directory!.FullName }, cancellationToken).ConfigureAwait(false);
                var workflow = await resources.GetAsync<Workflow>(workflowDefinition.Document.Name, workflowDefinition.Document.Namespace, cancellationToken).ConfigureAwait(false);
                if (workflow == null)
                {
                    workflow = new()
                    {
                        Metadata = new()
                        {
                            Namespace = workflowDefinition.Document.Namespace,
                            Name = workflowDefinition.Document.Name
                        },
                        Spec = new()
                        {
                            Versions = [workflowDefinition]
                        }
                    };
                    await resources.AddAsync(workflow, false, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var version = workflow.Spec.Versions.Get(workflowDefinition.Document.Version);
                    if (version != null)
                    {
                        if (this.Options.Seeding.Overwrite)
                        {
                            workflow.Spec.Versions.Remove(version);
                            workflow.Spec.Versions.Add(workflowDefinition);
                            await resources.ReplaceAsync(workflow, false, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            this.Logger.LogInformation("Skipped the import of workflow '{workflow}' from file '{file}' because it already exists", $"{workflowDefinition.Document.Name}.{workflowDefinition.Document.Namespace}:{workflowDefinition.Document.Version}", file.FullName);
                            continue;
                        }
                    }
                }
                this.Logger.LogInformation("Successfully imported workflow '{workflow}' from file '{file}'", $"{workflowDefinition.Document.Name}.{workflowDefinition.Document.Namespace}:{workflowDefinition.Document.Version}", file.FullName);
                count++;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("An error occurred while reading a workflow definition from file '{file}': {ex}", file.FullName, ex);
                continue;
            }
        }
        stopwatch.Stop();
        this.Logger.LogInformation("Completed importing {count} workflows in {ms} milliseconds", count, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Provisions functions from statis resource files
    /// </summary>
    /// <param name="resources">The <see cref="IResourceRepository"/> used to manage <see cref="IResource"/>s</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ProvisionFunctionsAsync(IResourceRepository resources, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        var directory = new DirectoryInfo(Path.Combine(this.Options.Seeding.Directory, "functions"));
        if (!directory.Exists) return;
        this.Logger.LogInformation("Starting importing custom functions from directory '{directory}'...", directory.FullName);
        var files = directory.GetFiles(this.Options.Seeding.FilePattern, SearchOption.AllDirectories).Where(f => f.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));
        if (!files.Any())
        {
            this.Logger.LogWarning("No custom function static resource files matching search pattern '{pattern}' found in directory '{directory}'. Skipping import.", this.Options.Seeding.FilePattern, directory.FullName);
            return;
        }
        stopwatch.Restart();
        var count = 0;
        foreach (var file in files)
        {
            try
            {
                var extension = file.FullName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                var serializer = extension?.ToLowerInvariant() switch
                {
                    "json" => (ITextSerializer)this.JsonSerializer,
                    "yml" or "yaml" => this.YamlSerializer,
                    _ => throw new NotSupportedException($"The specified extension '{extension}' is not supported for static resource files")
                };
                using var stream = file.OpenRead();
                using var streamReader = new StreamReader(stream);
                var text = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                var func = serializer.Deserialize<CustomFunction>(text)!;
                await resources.AddAsync(func, false, cancellationToken).ConfigureAwait(false);
                this.Logger.LogInformation("Successfully imported custom function '{customFunction}' from file '{file}'", func.GetQualifiedName(), file.FullName);
                count++;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("An error occurred while reading a custom function from file '{file}': {ex}", file.FullName, ex);
                continue;
            }
        }
        stopwatch.Stop();
        this.Logger.LogInformation("Completed importing {count} custom functions in {ms} milliseconds", count, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
