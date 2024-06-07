// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

using Neuroglia.Reactive;
using Synapse.Resources;
using System.Reactive.Linq;

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the service used to manage <see cref="Workflow"/> resources
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="controllerOptions">The service used to access the current <see cref="IOptions{TOptions}"/></param>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
/// <param name="operatorOptions">The current <see cref="Configuration.OperatorOptions"/></param>
/// <param name="operatorAccessor">The service used to access the current <see cref="Resources.Operator"/></param>
public class WorkflowController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<Workflow>> controllerOptions, IResourceRepository resources, IOptions<OperatorOptions> operatorOptions, IOperatorController operatorAccessor)
    : ResourceController<Workflow>(loggerFactory, controllerOptions, resources)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the running <see cref="Resources.Operator"/>'s options
    /// </summary>
    protected OperatorOptions OperatorOptions { get; } = operatorOptions.Value;

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Resources.Operator"/>
    /// </summary>
    protected IResourceMonitor<Resources.Operator> Operator => operatorAccessor.Operator;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains current <see cref="WorkflowScheduler"/>s
    /// </summary>
    protected ConcurrentDictionary<string, WorkflowScheduler> Schedulers { get; } = [];

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
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
        if (this.Operator == null) throw new Exception("The controller must be started before attempting any operation");
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
        await base.OnResourceCreatedAsync(resource, cancellationToken).ConfigureAwait(false);
        var definition = resource.Spec.Versions.GetLatest();
        if (definition.Schedule == null) return;
        if (!await this.TryClaimAsync(resource, cancellationToken).ConfigureAwait(false)) return;
        var scheduler = await this.CreateSchedulerAsync(resource, definition, cancellationToken).ConfigureAwait(false);
        await scheduler.ScheduleAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceUpdatedAsync(Workflow resource, CancellationToken cancellationToken = default)
    {
        await base.OnResourceUpdatedAsync(resource, cancellationToken).ConfigureAwait(false);
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
        await base.OnResourceDeletedAsync(resource, cancellationToken).ConfigureAwait(false);
        if (this.Schedulers.TryRemove(resource.GetQualifiedName(), out var scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        await foreach(var instance in this.Repository.GetAllAsync<WorkflowInstance>(cancellationToken: cancellationToken))
        {
            await this.Repository.RemoveAsync<WorkflowInstance>(instance.GetName(), instance.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles changes to the current operator's subscription selector
    /// </summary>
    /// <param name="selector">A key/value mapping of the labels both workflows and workflow instances to select must define</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceSelectorChangedAsync(IDictionary<string, string>? selector) => this.ReconcileAsync(this.CancellationTokenSource.Token);

}
