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

using Json.Patch;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Reactive;
using System.Threading;

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
        this.Operator!.Select(b => b.Resource.Spec.Selector).DistinctUntilChanged().SubscribeAsync(this.OnResourceSelectorChangedAsync, cancellationToken: cancellationToken);
        this.Where(e => e.Type == ResourceWatchEventType.Updated)
            .Select(e => new { Workflow = e.Resource, e.Resource.Spec.Versions })
            .DistinctUntilChanged()
            .Scan((Previous: (EquatableList<WorkflowDefinition>)null!, Current: (EquatableList<WorkflowDefinition>)null!, Workflow: (Workflow)null!), (accumulator, current) => (accumulator.Current, current.Versions, current.Workflow))
            .SubscribeAsync(async value => await this.OnWorkflowVersionChangedAsync(value.Workflow, value.Previous, value.Current).ConfigureAwait(false), cancellationToken: cancellationToken);
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
    /// <param name="workflow">The resource of the workflow to create a new scheduler for</param>
    /// <param name="definition">The definition of the workflow to create a new scheduler for</param>
    /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/></param>
    /// <returns>A new <see cref="WorkflowScheduler"/></returns>
    protected virtual async Task<WorkflowScheduler> CreateSchedulerAsync(Workflow workflow, WorkflowDefinition definition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(definition);
        var workflowMonitor = new ResourceMonitor<Workflow>(this.Watch ?? await this.Repository.WatchAsync<Workflow>(cancellationToken: cancellationToken).ConfigureAwait(false), workflow, this.Watch != null);
        var scheduler = ActivatorUtilities.CreateInstance<WorkflowScheduler>(this.ServiceProvider, workflowMonitor, definition);
        if (!this.Schedulers.TryAdd(workflow.GetQualifiedName(), scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        return scheduler;
    }

    /// <summary>
    /// Attempts to claim the specified <see cref="Workflow"/>
    /// </summary>
    /// <param name="workflow">The <see cref="Workflow"/> to try to claim</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be claimed</returns>
    protected virtual async Task<bool> TryClaimAsync(Workflow workflow, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        if (this.Operator == null) throw new Exception("The controller must be started before attempting any operation");
        if (workflow.Metadata.Labels != null && workflow.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorQualifiedName) && operatorQualifiedName == this.Operator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = workflow.Clone();
            workflow.Metadata.Labels ??= new Dictionary<string, string>();
            workflow.Metadata.Labels[SynapseDefaults.Resources.Labels.Operator] = this.Operator.Resource.GetQualifiedName();
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, workflow);
            workflow = await this.Repository.PatchAsync<Workflow>(new(PatchType.JsonPatch, patch), workflow.GetName(), workflow.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
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
    /// <param name="workflow">The <see cref="Workflow"/> to try to release</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be released</returns>
    protected virtual async Task<bool> TryReleaseAsync(Workflow workflow, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        if (workflow.Metadata.Labels != null && workflow.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out var operatorQualifiedName) && operatorQualifiedName == this.Operator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = workflow.Clone();
            workflow.Metadata.Labels ??= new Dictionary<string, string>();
            workflow.Metadata.Labels.Remove(SynapseDefaults.Resources.Labels.Operator);
            if (workflow.Metadata.Labels.Count < 1) workflow.Metadata.Labels = null;
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, workflow);
            workflow = await this.Repository.PatchAsync<Workflow>(new(PatchType.JsonPatch, patch), workflow.GetName(), workflow.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
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
    protected override async Task OnResourceCreatedAsync(Workflow workflow, CancellationToken cancellationToken)
    {
        await base.OnResourceCreatedAsync(workflow, cancellationToken).ConfigureAwait(false);
        var definition = workflow.Spec.Versions.GetLatest();
        if (definition.Schedule == null) return;
        if (!await this.TryClaimAsync(workflow, cancellationToken).ConfigureAwait(false)) return;
        var scheduler = await this.CreateSchedulerAsync(workflow, definition, cancellationToken).ConfigureAwait(false);
        await scheduler.ScheduleAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(Workflow workflow, CancellationToken cancellationToken)
    {
        await base.OnResourceDeletedAsync(workflow, cancellationToken).ConfigureAwait(false);
        if (this.Schedulers.TryRemove(workflow.GetQualifiedName(), out var scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        await foreach(var instance in this.Repository.GetAllAsync<WorkflowInstance>(cancellationToken: cancellationToken))
        {
            await this.Repository.RemoveAsync<WorkflowInstance>(instance.GetName(), instance.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles changes to the specified workflow's versions
    /// </summary>
    /// <param name="workflow">The updated workflow</param>
    /// <param name="previous">The previous versions</param>
    /// <param name="current">The actual versions</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnWorkflowVersionChangedAsync(Workflow workflow, EquatableList<WorkflowDefinition> previous, EquatableList<WorkflowDefinition> current)
    {
        if (previous == null) return;
        if (workflow.Metadata.Labels == null || !workflow.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Operator, out _)) if (!await this.TryClaimAsync(workflow, this.CancellationTokenSource.Token).ConfigureAwait(false)) return;
        if (workflow.Metadata.Labels?[SynapseDefaults.Resources.Labels.Operator] != this.Operator.Resource.GetQualifiedName()) return;
        var diffPatch = JsonPatchUtility.CreateJsonPatchFromDiff(previous, current);
        var operation = diffPatch.Operations[0].Op;
        if (this.Schedulers.TryRemove(workflow.GetQualifiedName(), out var scheduler)) await scheduler.DisposeAsync().ConfigureAwait(false);
        if (operation == OperationType.Remove) return;
        var definition = workflow.Spec.Versions.GetLatest();
        if (definition.Schedule == null) return;
        scheduler = await this.CreateSchedulerAsync(workflow, definition, this.CancellationTokenSource.Token).ConfigureAwait(false);
        await scheduler.ScheduleAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles changes to the current operator's subscription selector
    /// </summary>
    /// <param name="selector">A key/value mapping of the labels both workflows and workflow instances to select must define</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceSelectorChangedAsync(IDictionary<string, string>? selector) => this.ReconcileAsync(this.CancellationTokenSource.Token);

}
