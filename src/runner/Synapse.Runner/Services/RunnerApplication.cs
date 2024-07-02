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

namespace Synapse.Runner.Services;

/// <summary>
/// Represents a service used to initialize the current <see cref="IWorkflowExecutor"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="applicationLifetime">The current <see cref="IHostApplicationLifetime"/></param>
/// <param name="options">The service used to access the current <see cref="RunnerOptions"/></param>
internal class RunnerApplication(IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime, IOptions<RunnerOptions> options)
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
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient ApiClient => this.ServiceProvider.GetRequiredService<ISynapseApiClient>();

    /// <summary>
    /// Gets the service used to access the current <see cref="RunnerOptions"/>
    /// </summary>
    protected RunnerOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets the service used to execute the workflow instance to run
    /// </summary>
    protected IWorkflowExecutor? Executor { get; private set; }

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.Options.Workflow?.Instance)) throw new NullReferenceException("The workflow instance to run must be configured, which can be done using the application's appsettings.json file, using command line arguments or using environment variables");
        var instance = await this.ApiClient.WorkflowInstances.GetAsync(this.Options.Workflow.GetInstanceName(), this.Options.Workflow.GetInstanceNamespace(), cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the specified workflow instance '{this.Options.Workflow.Instance}'");
        var resource = await this.ApiClient.Workflows.GetAsync(instance.Spec.Definition.Name, instance.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the specified workflow '{instance.Spec.Definition.Namespace}.{instance.Spec.Definition.Name}'");
        var definition = resource.Spec.Versions.FirstOrDefault(v => v.Document.Version == instance.Spec.Definition.Version) ?? throw new NullReferenceException($"Failed to find the specified version '{instance.Spec.Definition.Version}' of the workflow '{instance.Spec.Definition.Namespace}.{instance.Spec.Definition.Name}'");
        var expressionLanguage = definition.Evaluate?.Language ?? RuntimeExpressions.Languages.JQ;
        var expressionEvaluator = this.ServiceProvider.GetRequiredService<IExpressionEvaluatorProvider>().GetEvaluator(expressionLanguage) 
            ?? throw new NullReferenceException($"Failed to find an expression evaluator for the language '{expressionLanguage}' defined by workflow '{instance.Spec.Definition.Namespace}.{instance.Spec.Definition.Name}:{instance.Spec.Definition.Version}'");
        var context = ActivatorUtilities.CreateInstance<WorkflowExecutionContext>(this.ServiceProvider, expressionEvaluator, definition, instance);
        this.Executor = ActivatorUtilities.CreateInstance<WorkflowExecutor>(this.ServiceProvider, context);
        await this.Executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
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
