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

using Neuroglia.Data.Infrastructure.Services;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to manage <see cref="WorkflowInstance"/> resources
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="controllerOptions">The service used to access the current <see cref="IOptions{TOptions}"/></param>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
/// <param name="operatorController">The service used to access the current <see cref="Resources.Operator"/></param>
/// <param name="workflowController">The service used to access all monitored <see cref="Workflow"/>s</param>
/// <param name="documents">The <see cref="IRepository"/> used to manage <see cref="Document"/>s</param>
public class WorkflowInstanceController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<WorkflowInstance>> controllerOptions, IResourceRepository repository, IOperatorController operatorController, IWorkflowController workflowController, IRepository<Document, string> documents)
    : ResourceController<WorkflowInstance>(loggerFactory, controllerOptions, repository)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Operator"/>
    /// </summary>
    protected IResourceMonitor<Resources.Operator> Operator => operatorController.Operator;

    /// <summary>
    /// Gets a dictionary containing all monitored <see cref="Workflow"/>s
    /// </summary>
    protected IReadOnlyDictionary<string, Workflow> Workflows => workflowController.Workflows;

    /// <summary>
    /// Gets the <see cref="IRepository"/> used to manage <see cref="Document"/>s
    /// </summary>
    protected IRepository<Document, string> Documents => documents;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains current <see cref="WorkflowInstanceHandler"/>es
    /// </summary>
    protected ConcurrentDictionary<string, WorkflowInstanceHandler> Handlers { get; } = [];

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
        this.Operator!.Select(b => b.Resource.Spec.Selector).SubscribeAsync(this.OnResourceSelectorChangedAsync, cancellationToken: cancellationToken);
        await this.OnResourceSelectorChangedAsync(this.Operator!.Resource.Spec.Selector).ConfigureAwait(false);
        if (this.Operator?.Resource?.Spec?.Cleanup != null)_ = Task.Run(() => this.CleanupAsync(), CancellationTokenSource.Token);
    }

    /// <inheritdoc/>
    protected override Task ReconcileAsync(CancellationToken cancellationToken = default)
    {
        this.Options.LabelSelectors = this.Operator?.Resource.Spec.Selector?.Select(s => new LabelSelector(s.Key, LabelSelectionOperator.Equals, s.Value)).ToList();
        return base.ReconcileAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected virtual async Task CleanupAsync()
    {
        while (!CancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                if (this.Operator?.Resource?.Spec?.Cleanup == null) break;
                var cutoff = DateTimeOffset.UtcNow - this.Operator?.Resource?.Spec?.Cleanup.Ttl;
                var deleted = 0;
                var selectors = this.Options.LabelSelectors;

                await foreach (var instance in this.Repository.GetAllAsync<WorkflowInstance>(labelSelectors: selectors, cancellationToken: CancellationTokenSource.Token))
                {
                    if (Handlers.ContainsKey(instance.GetQualifiedName())) continue;
                    if (instance.IsOperative) continue;
                    var finishedAt = instance.Status?.EndedAt ?? instance.Metadata.CreationTimestamp;
                    if (finishedAt <= cutoff && !this.Handlers.ContainsKey(instance.GetQualifiedName()))
                    {
                        try { await this.TryReleaseAsync(instance, CancellationTokenSource.Token).ConfigureAwait(false); } catch { }
                        try
                        {
                            await this.Repository.RemoveAsync<WorkflowInstance>(instance.GetName(), instance.GetNamespace(), false, CancellationTokenSource.Token).ConfigureAwait(false);
                            deleted++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Failed to delete expired workflow instance {instance}", instance.GetQualifiedName());
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (CancellationTokenSource.Token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Instance cleanup sweep failed");
                try { await Task.Delay(TimeSpan.FromSeconds(5), CancellationTokenSource.Token).ConfigureAwait(false); } catch { }
            }
            try
            {
                var delay = this.Operator?.Resource?.Spec?.Cleanup?.Interval ?? TimeSpan.FromMinutes(5);
                await Task.Delay(delay, CancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (CancellationTokenSource.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="WorkflowInstanceHandler"/> for the specified workflow
    /// </summary>
    /// <param name="workflowInstance">The workflow instance to create a new handler for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="WorkflowInstanceHandler"/></returns>
    protected virtual async Task<WorkflowInstanceHandler> CreateWorkflowInstanceHandlerAsync(WorkflowInstance workflowInstance, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workflowInstance);
        var correlationMonitor = new ResourceMonitor<WorkflowInstance>(this.Watch ?? await this.Repository.WatchAsync<WorkflowInstance>(cancellationToken: cancellationToken).ConfigureAwait(false), workflowInstance, this.Watch != null);
        var handler = ActivatorUtilities.CreateInstance<WorkflowInstanceHandler>(this.ServiceProvider, correlationMonitor);
        if (!this.Handlers.TryAdd(workflowInstance.GetQualifiedName(), handler)) await handler.DisposeAsync().ConfigureAwait(false);
        return handler;
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
        var isClaimable = this.IsWorkflowInstanceClaimable(resource);
        if (isClaimable.HasValue) return isClaimable.Value;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels[SynapseDefaults.Resources.Labels.Operator] = this.Operator.Resource.GetQualifiedName();
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<WorkflowInstance>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
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
        var isClaimable = this.IsWorkflowInstanceClaimable(resource);
        if (isClaimable.HasValue) return isClaimable.Value;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels.Remove(SynapseDefaults.Resources.Labels.Operator);
            if (resource.Metadata.Labels.Count < 1) resource.Metadata.Labels = null;
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<WorkflowInstance>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
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
        foreach(var process in this.Handlers.Values)
        {
            await process.DisposeAsync().ConfigureAwait(false);
        }
        this.Handlers.Clear();
        await foreach(var resource in this.Repository.GetAllAsync<WorkflowInstance>(labelSelectors: [new LabelSelector(SynapseDefaults.Resources.Labels.Operator, LabelSelectionOperator.Equals, [this.Operator.Resource.GetQualifiedName()])], cancellationToken: cancellationToken))
        {
            await this.TryReleaseAsync(resource, cancellationToken).ConfigureAwait(false);
        }
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceCreatedAsync(WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.OnResourceCreatedAsync(workflowInstance, cancellationToken).ConfigureAwait(false);
            if (!await this.TryClaimAsync(workflowInstance, cancellationToken).ConfigureAwait(false)) return;
            var handler = await this.CreateWorkflowInstanceHandlerAsync(workflowInstance, cancellationToken).ConfigureAwait(false);
            await handler.HandleAsync(cancellationToken).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while handling the creation of workflow instance '{workflowInstance}': {ex}", workflowInstance.GetQualifiedName(), ex);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.OnResourceDeletedAsync(workflowInstance, cancellationToken).ConfigureAwait(false);
            if (this.Handlers.TryRemove(workflowInstance.GetQualifiedName(), out var process)) await process.DisposeAsync().ConfigureAwait(false);
            var selectors = new LabelSelector[]
            {
            new(SynapseDefaults.Resources.Labels.WorkflowInstance, LabelSelectionOperator.Equals, workflowInstance.GetQualifiedName())
            };
            await foreach (var correlation in this.Repository.GetAllAsync<Correlation>(null, selectors, cancellationToken: cancellationToken))
            {
                await this.Repository.RemoveAsync<Correlation>(correlation.GetName(), correlation.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
            }
            if (workflowInstance.Status != null)
            {
                var documentReferences = new List<string>();
                if (!string.IsNullOrWhiteSpace(workflowInstance.Status.ContextReference)) documentReferences.Add(workflowInstance.Status.ContextReference);
                if (!string.IsNullOrWhiteSpace(workflowInstance.Status.OutputReference)) documentReferences.Add(workflowInstance.Status.OutputReference);
                if (workflowInstance.Status.Tasks != null)
                {
                    foreach (var task in workflowInstance.Status.Tasks)
                    {
                        if (!string.IsNullOrWhiteSpace(task.ContextReference)) documentReferences.Add(task.ContextReference);
                        if (!string.IsNullOrWhiteSpace(task.InputReference)) documentReferences.Add(task.InputReference);
                        if (!string.IsNullOrWhiteSpace(task.OutputReference)) documentReferences.Add(task.OutputReference);
                    }
                }
                foreach (var documentReference in documentReferences.Distinct()) await this.Documents.RemoveAsync(documentReference, cancellationToken).ConfigureAwait(false);
            }
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occured while handling the deletion of workflow instance '{workflowInstance}': {ex}", workflowInstance.GetQualifiedName(), ex);
        }
    }

    /// <summary>
    /// Handles changes to the current operator's subscription selector
    /// </summary>
    /// <param name="selector">A key/value mapping of the labels both workflows and workflow instances to select must define</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceSelectorChangedAsync(IDictionary<string, string>? selector) => this.ReconcileAsync(this.CancellationTokenSource.Token);

    /// <summary>
    /// Determines whether or not the specified <see cref="WorkflowInstance"/> can be claimed by the current <see cref="Resources.Operator"/>
    /// </summary>
    /// <param name="workflowInstance">The <see cref="WorkflowInstance"/> to check</param>
    /// <returns>A boolean indicating whether or not the specified <see cref="WorkflowInstance"/> can be claimed by the current <see cref="Resources.Operator"/></returns>
    protected virtual bool? IsWorkflowInstanceClaimable(WorkflowInstance workflowInstance)
    {
        ArgumentNullException.ThrowIfNull(workflowInstance);
        if (workflowInstance.Metadata.Labels != null && workflowInstance.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorQualifiedName)) return operatorQualifiedName == this.Operator.Resource.GetQualifiedName();
        if (this.Workflows.TryGetValue(this.GetResourceCacheKey(workflowInstance.Spec.Definition.Name, workflowInstance.Spec.Definition.Namespace), out var workflow))
        {
            if (workflow.Metadata.Labels != null && workflow.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out operatorQualifiedName)) return operatorQualifiedName == this.Operator.Resource.GetQualifiedName();
        }
        else
        {
            // if we are not able to retrieve the workflow, we assume it has been ignored by selectors, and thus cannot be claimed
            return false;
        }
        return null;
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await this.StopAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
        await base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        foreach(var handler in this.Handlers.Values) handler.Dispose();
        this.Handlers.Clear();
        base.Dispose(disposing);
    }

}
