namespace Synapse.Operator.Application.Services;

/// <summary>
/// Represents the service used to manage <see cref="Workflow"/> resources
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="controllerOptions">The service used to access the current <see cref="IOptions{TOptions}"/></param>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
public class WorkflowController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<Workflow>> controllerOptions, IResourceRepository resources, IResourceMonitor<Resources.Operator> @operator)
    : ResourceController<Workflow>(loggerFactory, controllerOptions, resources)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Operator"/>
    /// </summary>
    protected IResourceMonitor<Resources.Operator> Operator { get; } = @operator;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains current <see cref="WorkflowScheduler"/>s
    /// </summary>
    protected ConcurrentDictionary<string, WorkflowScheduler> Schedulers { get; } = [];

    /// <summary>
    /// Creates a new <see cref="WorkflowScheduler"/> for the specified workflow
    /// </summary>
    /// <param name="resource">The resource of the workflow to create a new scheduler for</param>
    /// <param name="definition">The definition of the workflow to create a new scheduler for</param>
    /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/></param>
    /// <returns>A new <see cref="WorkflowScheduler"/></returns>
    protected virtual async Task<WorkflowScheduler> CreateSchedulerAsync(Workflow resource, WorkflowDefinition definition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(definition);
        var scheduler = ActivatorUtilities.CreateInstance<WorkflowScheduler>(this.ServiceProvider, resource, definition);
        if (!this.Schedulers.TryAdd(resource.GetQualifiedName(), scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        return scheduler;
    }

    /// <summary>
    /// Attempts to claim the specified <see cref="Workflow"/>
    /// </summary>
    /// <param name="resource">The <see cref="Workflow"/> to try to claim</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be claimed</returns>
    protected virtual async Task<bool> TryClaimAsync(Workflow resource, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.Metadata.Labels != null && resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorQualifiedName) && operatorQualifiedName == this.Operator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels[SynapseDefaults.Resources.Labels.Operator] = this.Operator.Resource.GetQualifiedName();
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<Workflow>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (ConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to release the specified <see cref="Workflow"/>
    /// </summary>
    /// <param name="resource">The <see cref="Workflow"/> to try to release</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be released</returns>
    protected virtual async Task<bool> TryReleaseAsync(Workflow resource, CancellationToken cancellationToken)
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
            resource = await this.Repository.PatchAsync<Workflow>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
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
        await foreach (var resource in this.Repository.GetAllAsync<Workflow>(labelSelectors: [new LabelSelector(SynapseDefaults.Resources.Labels.Operator, LabelSelectionOperator.Equals, [this.Operator.Resource.GetQualifiedName()])], cancellationToken: cancellationToken))
        {
            await this.TryReleaseAsync(resource, cancellationToken).ConfigureAwait(false);
        }
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceCreatedAsync(Workflow resource, CancellationToken cancellationToken = default)
    {
        if (!await this.TryClaimAsync(resource, cancellationToken).ConfigureAwait(false)) return;
        var definition = resource.Spec.Versions.GetLatest();
        if (definition.Schedule == null) return;
        var scheduler = await this.CreateSchedulerAsync(resource, definition, cancellationToken).ConfigureAwait(false);
        await scheduler.ScheduleAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceUpdatedAsync(Workflow resource, CancellationToken cancellationToken = default)
    {
        if (resource.Metadata.Labels == null || !resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out _)) if (!await this.TryClaimAsync(resource, cancellationToken).ConfigureAwait(false)) return;
        if (resource.Metadata.Labels?[SynapseDefaults.Resources.Labels.Operator] != this.Operator.Resource.GetQualifiedName()) return;
        if (this.Schedulers.TryRemove(resource.GetQualifiedName(), out var scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        var definition = resource.Spec.Versions.GetLatest();
        if (definition.Schedule == null) return;
        scheduler = await this.CreateSchedulerAsync(resource, definition, cancellationToken).ConfigureAwait(false);
        await scheduler.ScheduleAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(Workflow resource, CancellationToken cancellationToken = default)
    {
        if (this.Schedulers.TryRemove(resource.GetQualifiedName(), out var scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        await foreach(var instance in this.Repository.GetAllAsync<WorkflowInstance>(cancellationToken: cancellationToken))
        {
            await this.Repository.RemoveAsync<WorkflowInstance>(instance.GetName(), instance.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
        }
    }

}
