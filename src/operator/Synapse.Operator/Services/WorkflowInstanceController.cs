using Neuroglia.Reactive;
using System.Reactive.Linq;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to manage <see cref="WorkflowInstance"/> resources
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="controllerOptions">The service used to access the current <see cref="IOptions{TOptions}"/></param>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
/// <param name="operatorAccessor">The service used to access the current <see cref="Resources.Operator"/></param>
public class WorkflowInstanceController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<WorkflowInstance>> controllerOptions, IResourceRepository repository, IOperatorController operatorAccessor)
    : ResourceController<WorkflowInstance>(loggerFactory, controllerOptions, repository)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Operator"/>
    /// </summary>
    protected IResourceMonitor<Resources.Operator> Operator => operatorAccessor.Operator;

    /// <summary>
    /// Gets the service used to create and run <see cref="IWorkflowProcess"/>es
    /// </summary>
    protected IWorkflowRuntime Runtime => this.ServiceProvider.GetRequiredService<IWorkflowRuntime>();

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains current <see cref="IWorkflowProcess"/>es
    /// </summary>
    protected ConcurrentDictionary<string, IWorkflowProcess> Processes { get; } = [];

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
        foreach (var workflowInstance in this.Resources.Values.ToList())
        {
            await this.OnResourceCreatedAsync(workflowInstance, cancellationToken).ConfigureAwait(false);
        }
        this.Operator!.Select(b => b.Resource.Spec.Selector).SubscribeAsync(this.OnResourceSelectorChangedAsync, cancellationToken: cancellationToken);
        await this.OnResourceSelectorChangedAsync(this.Operator!.Resource.Spec.Selector).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override Task ReconcileAsync(CancellationToken cancellationToken = default)
    {
        this.Options.LabelSelectors = this.Operator?.Resource.Spec.Selector?.Select(s => new LabelSelector(s.Key, LabelSelectionOperator.Equals, s.Value)).ToList();
        return base.ReconcileAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new <see cref="IWorkflowProcess"/> for the specified workflow
    /// </summary>
    /// <param name="workflow">The definition of the workflow to create a new process for</param>
    /// <param name="workflowInstance">The workflow instance to create a new process for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IWorkflowProcess"/></returns>
    protected virtual async Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(workflowInstance);
        var process = await this.Runtime.CreateProcessAsync(workflow, workflowInstance, cancellationToken).ConfigureAwait(false);
        if (!this.Processes.TryAdd(workflowInstance.GetQualifiedName(), process)) await process.DisposeAsync().ConfigureAwait(false);
        return process;
    }

    /// <summary>
    /// Attempts to claim the specified <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="resource">The <see cref="WorkflowInstance"/> to try to claim</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be claimed</returns>
    protected virtual async Task<bool> TryClaimAsync(WorkflowInstance resource, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.Metadata.Labels != null && resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorQualifiedName) && operatorQualifiedName == this.Operator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels[SynapseDefaults.Resources.Labels.Operator] = this.Operator.Resource.GetQualifiedName();
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<WorkflowInstance>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (ConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to release the specified <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="resource">The <see cref="WorkflowInstance"/> to try to release</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be released</returns>
    protected virtual async Task<bool> TryReleaseAsync(WorkflowInstance resource, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.Metadata.Labels != null && resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorQualifiedName) && operatorQualifiedName == this.Operator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels.Remove(SynapseDefaults.Resources.Labels.Operator);
            if (resource.Metadata.Labels.Count < 1) resource.Metadata.Labels = null;
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<WorkflowInstance>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (ConcurrencyException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach(var process in this.Processes.Values)
        {
            await process.StopAsync(cancellationToken).ConfigureAwait(false);
            await process.DisposeAsync().ConfigureAwait(false);
        }
        this.Processes.Clear();
        await foreach(var resource in this.Repository.GetAllAsync<WorkflowInstance>(labelSelectors: [new LabelSelector(SynapseDefaults.Resources.Labels.Operator, LabelSelectionOperator.Equals, [this.Operator.Resource.GetQualifiedName()])], cancellationToken: cancellationToken))
        {
            await this.TryReleaseAsync(resource, cancellationToken).ConfigureAwait(false);
        }
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceCreatedAsync(WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
    {
        if (!await this.TryClaimAsync(workflowInstance, cancellationToken).ConfigureAwait(false)) return;
        var workflow = await this.Repository.GetAsync<Workflow>(workflowInstance.Spec.Definition.Name, workflowInstance.Spec.Definition.Namespace, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException($"Failed to find the workflow with name '{workflowInstance.Spec.Definition.Namespace}.{workflowInstance.Spec.Definition.Name}'");
        var process = await this.Runtime.CreateProcessAsync(workflow, workflowInstance, cancellationToken).ConfigureAwait(false);
        await process.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
    {
        if (this.Processes.TryRemove(workflowInstance.GetQualifiedName(), out var process)) await process.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles changes to the current operator's subscription selector
    /// </summary>
    /// <param name="selector">A key/value mapping of the labels both workflows and workflow instances to select must define</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceSelectorChangedAsync(IDictionary<string, string>? selector) => this.ReconcileAsync(this.CancellationTokenSource.Token);

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await this.StopAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
        await base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        foreach(var handler in this.Processes.Values) handler.Dispose();
        this.Processes.Clear();
        base.Dispose(disposing);
    }

}
