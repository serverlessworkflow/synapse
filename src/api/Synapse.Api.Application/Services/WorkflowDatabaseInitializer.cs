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
/// <param name="workflowDefinitionReader">The service used to read <see cref="WorkflowDefinition"/>s</param>
/// <param name="options">The service used to access the current <see cref="ApiServerOptions"/></param>
public class WorkflowDatabaseInitializer(IServiceProvider serviceProvider, ILogger<WorkflowDatabaseInitializer> logger, IWorkflowDefinitionReader workflowDefinitionReader, IOptions<ApiServerOptions> options)
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
        this.Logger.LogInformation("Starting importing static resources from directory '{directory}'...", directory.FullName);
        var files = directory.GetFiles(this.Options.Seeding.FilePattern, SearchOption.AllDirectories).Where(f => f.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) || f.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));
        if (!files.Any())
        {
            this.Logger.LogWarning("No static resource files matching search pattern '{pattern}' found in directory '{directory}'. Skipping import.", this.Options.Seeding.FilePattern, directory.FullName);
            return;
        }
        var count = 0;
        stopwatch.Restart();
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
                    if (await resources.GetAsync<Namespace>(workflow.GetNamespace()!, cancellationToken: cancellationToken).ConfigureAwait(false) == null)
                    {
                        await resources.AddAsync(new Namespace() { Metadata = new() { Name = workflow.GetNamespace()! } }, false, cancellationToken).ConfigureAwait(false);
                        this.Logger.LogInformation("Successfully created namespace '{namespace}'", workflow.GetNamespace());
                    }
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
            catch(Exception ex)
            {
                this.Logger.LogError("An error occurred while reading a workflow definition from file '{file}': {ex}", file.FullName, ex);
                continue;
            }
        }
        stopwatch.Stop();
        this.Logger.LogInformation("Completed importing {count} static resources in {ms} milliseconds", count, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
