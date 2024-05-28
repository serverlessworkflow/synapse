using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Neuroglia.Data.Expressions.Services;
using Synapse.Api.Client.Services;
using Synapse.Runner.Configuration;

namespace Synapse.Runner.Services;

/// <summary>
/// Represents a service used to initialize the current <see cref="IWorkflowExecutor"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="options">The service used to access the current <see cref="RunnerOptions"/></param>
public class WorkflowExecutorInitializer(IServiceProvider serviceProvider, ISynapseApiClient apiClient, IOptions<RunnerOptions> options)
    : IHostedService
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient ApiClient { get; } = apiClient;

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

    }

    /// <inheritdoc/>
    public virtual async Task StopAsync(CancellationToken cancellationToken) 
    {
        if(this.Executor != null)
        {
            await this.Executor.CancelAsync(cancellationToken).ConfigureAwait(false);
            await this.Executor.DisposeAsync().ConfigureAwait(false);
            this.Executor = null;
        }
    }

}
