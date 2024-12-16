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

using MimeKit;
using Neuroglia;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using ServerlessWorkflow.Sdk.IO;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents a service used to initialize the current <see cref="IWorkflowExecutor"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="applicationLifetime">The current <see cref="IHostApplicationLifetime"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="options">The service used to access the current <see cref="RunnerOptions"/></param>
internal class RunnerApplication(IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime, ILogger<RunnerApplication> logger, IOptions<RunnerOptions> options)
    : IHostedService, IDisposable
{

    bool _disposed;

    /// <summary>
    /// Gets the current <see cref="IServiceScope"/>
    /// </summary>
    protected IServiceScope ServiceScope { get; } = serviceProvider.CreateScope();

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider => this.ServiceScope.ServiceProvider;

    /// <summary>
    /// Gets the current <see cref="IHostApplicationLifetime"/>
    /// </summary>
    protected IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient ApiClient => this.ServiceProvider.GetRequiredService<ISynapseApiClient>();

    /// <summary>
    /// Gets the service used to read <see cref="WorkflowDefinition"/>s
    /// </summary>
    protected IWorkflowDefinitionReader WorkflowDefinitionReader => this.ServiceProvider.GetRequiredService<IWorkflowDefinitionReader>();

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer => this.ServiceProvider.GetRequiredService<IJsonSerializer>();

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from YAML
    /// </summary>
    protected IYamlSerializer YamlSerializer => this.ServiceProvider.GetRequiredService<IYamlSerializer>();

    /// <summary>
    /// Gets the service used to access the current <see cref="RunnerOptions"/>
    /// </summary>
    protected RunnerOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets the <see cref="IObservable{T}"/> used to monitor the workflow instance to run
    /// </summary>
    protected IObservable<IResourceWatchEvent<WorkflowInstance>>? Events { get; private set; }

    /// <summary>
    /// Gets the service used to execute the workflow instance to run
    /// </summary>
    protected IWorkflowExecutor? Executor { get; private set; }

    /// <inheritdoc/>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        this.ApplicationLifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(async () => await this.RunAsync(cancellationToken), cancellationToken);
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Runs the applications
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            WorkflowDefinition definition;
            WorkflowInstance instance;
            Type executionContextType;
            switch (this.Options.ExecutionMode)
            {
                case RunnerExecutionMode.Connected:
                    if (string.IsNullOrWhiteSpace(this.Options.Workflow.InstanceQualifiedName)) throw new NullReferenceException("The workflow instance to run must be configured, which can be done using the application's appsettings.json file, using command line arguments or using environment variables");
                    instance = await this.ApiClient.WorkflowInstances.GetAsync(this.Options.Workflow.GetInstanceName(), this.Options.Workflow.GetInstanceNamespace(), cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the specified workflow instance '{this.Options.Workflow.InstanceQualifiedName}'");
                    var resource = await this.ApiClient.Workflows.GetAsync(instance.Spec.Definition.Name, instance.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the specified workflow '{instance.Spec.Definition.Namespace}.{instance.Spec.Definition.Name}'");
                    definition = resource.Spec.Versions.FirstOrDefault(v => v.Document.Version == instance.Spec.Definition.Version) ?? throw new NullReferenceException($"Failed to find the specified version '{instance.Spec.Definition.Version}' of the workflow '{instance.Spec.Definition.Namespace}.{instance.Spec.Definition.Name}'");
                    executionContextType = typeof(ConnectedWorkflowExecutionContext);
                    break;
                case RunnerExecutionMode.StandAlone:
                    if (string.IsNullOrWhiteSpace(this.Options.Workflow.DefinitionFilePath)) throw new NullReferenceException("The path to the workflow definition file must be set in stand-alone execution mode");
                    if (!File.Exists(this.Options.Workflow.DefinitionFilePath)) throw new FileNotFoundException("The workflow definition file does not exist or cannot be found", this.Options.Workflow.DefinitionFilePath);
                    var stream = new FileStream(this.Options.Workflow.DefinitionFilePath, FileMode.Open);
                    var readerOptions = new WorkflowDefinitionReaderOptions();
                    definition = await this.WorkflowDefinitionReader.ReadAsync(stream, readerOptions, cancellationToken).ConfigureAwait(false);
                    await stream.DisposeAsync().ConfigureAwait(false);
                    IDictionary<string, object>? input = null;
                    if (!string.IsNullOrWhiteSpace(this.Options.Workflow.InputFilePath))
                    {
                        var inputFile = new FileInfo(this.Options.Workflow.InputFilePath);
                        if (!inputFile.Exists) throw new FileNotFoundException("The workflow input file does not exist or cannot be found", this.Options.Workflow.InputFilePath);
                        var extension = inputFile.Extension.ToLowerInvariant().Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
                        stream = inputFile.OpenRead();
                        input = extension switch
                        {
                            "json" => this.JsonSerializer.Deserialize<IDictionary<string, object>>(stream),
                            "yaml" or "yml" => this.YamlSerializer.Deserialize<IDictionary<string, object>>(stream),
                            _ => throw new NotSupportedException($"The workflow input file extension '{extension}' is not supported. Supported extensions are '.json', '.yaml' and '.yml'"),
                        };
                        await stream.DisposeAsync().ConfigureAwait(false);
                    }
                    instance = new WorkflowInstance()
                    {
                        Metadata = new()
                        {
                            Namespace = definition.Document.Namespace,
                            Name = $"{definition.Document.Name}-{Guid.NewGuid().ToShortString()}"
                        },
                        Spec = new()
                        {
                            Definition = new()
                            {
                                Namespace = definition.Document.Namespace,
                                Name = definition.Document.Name,
                                Version = definition.Document.Version
                            },
                            Input = input == null ? null : new(input)
                        }
                    };
                    executionContextType = typeof(StandAloneWorkflowExecutionContext);
                    break;
                default:
                    throw new NotSupportedException($"The specified runner execution mode '{this.Options.ExecutionMode}' is not supported");
            }
            var expressionLanguage = definition.Evaluate?.Language ?? RuntimeExpressions.Languages.JQ;
            var expressionEvaluator = this.ServiceProvider.GetRequiredService<IExpressionEvaluatorProvider>().GetEvaluator(expressionLanguage)
                ?? throw new NullReferenceException($"Failed to find an expression evaluator for the language '{expressionLanguage}' defined by workflow '{instance.Spec.Definition.Namespace}.{instance.Spec.Definition.Name}:{instance.Spec.Definition.Version}'");
            var context = (IWorkflowExecutionContext)ActivatorUtilities.CreateInstance(this.ServiceProvider, executionContextType, expressionEvaluator, definition, instance);
            this.Events = this.ApiClient.WorkflowInstances.MonitorAsync(instance.Metadata.Name!, instance.Metadata.Namespace!, cancellationToken).ToObservable();
            this.Events
                .Where(e => e.Type == ResourceWatchEventType.Updated && e.Resource.Status?.Phase != context.Instance.Status?.Phase)
                .Select(e => e.Resource.Status?.Phase)
                .SubscribeAsync(async phase => await this.OnHandleStatusPhaseChangedAsync(phase, cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken);
            this.Executor = ActivatorUtilities.CreateInstance<WorkflowExecutor>(this.ServiceProvider, context);
            await this.Executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while running the specified workflow instance: {ex}", ex);
        }
        this.ApplicationLifetime.StopApplication();
    }

    /// <inheritdoc/>
    public virtual async Task StopAsync(CancellationToken cancellationToken) 
    {
        if(this.Executor != null)
        {
            await this.Executor.DisposeAsync().ConfigureAwait(false);
            this.Executor = null;
        }
    }

    protected virtual async Task OnHandleStatusPhaseChangedAsync(string? phase, CancellationToken cancellationToken)
    {
        switch (phase)
        {
            case WorkflowInstanceStatusPhase.Suspended:
                await this.OnSuspendAsync(cancellationToken).ConfigureAwait(false);
                break;
            case WorkflowInstanceStatusPhase.Cancelled:
                await this.OnCancelAsync(cancellationToken).ConfigureAwait(false);
                break;
            default:
                return;
        }
    }

    protected virtual async Task OnSuspendAsync(CancellationToken cancellationToken)
    {
        if (this.Executor == null)
        {
            this.ApplicationLifetime.StopApplication();
            return;
        }
        await this.Executor.SuspendAsync(cancellationToken).ConfigureAwait(false);
    }

    protected virtual async Task OnCancelAsync(CancellationToken cancellationToken)
    {
        if (this.Executor == null)
        {
            this.ApplicationLifetime.StopApplication();
            return;
        }
        await this.Executor.CancelAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="RunnerApplication"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="RunnerApplication"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || this._disposed) return;
        this.ServiceScope.Dispose();
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}
