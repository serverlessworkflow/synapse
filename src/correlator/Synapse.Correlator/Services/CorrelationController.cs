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

using System.Collections.Concurrent;

namespace Synapse.Correlator.Services;

/// <summary>
/// Represents the service used to manage <see cref="Correlation"/> resources
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="controllerOptions">The service used to access the current <see cref="IOptions{TOptions}"/></param>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
/// <param name="expressionEvaluatorProvider">The service used to provide <see cref="IExpressionEvaluator"/>s</param>
/// <param name="correlatorOptions">The current <see cref="Configuration.CorrelatorOptions"/></param>
/// <param name="correlatorAccessor">The service used to access the current <see cref="Resources.Correlator"/></param>
public class CorrelationController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<Correlation>> controllerOptions, IResourceRepository resources, IExpressionEvaluatorProvider expressionEvaluatorProvider, IOptions<CorrelatorOptions> correlatorOptions, ICorrelatorController correlatorAccessor)
    : ResourceController<Correlation>(loggerFactory, controllerOptions, resources)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the service used to provide <see cref="IExpressionEvaluator"/>s
    /// </summary>
    protected IExpressionEvaluatorProvider ExpressionEvaluatorProvider { get; } = expressionEvaluatorProvider;

    /// <summary>
    /// Gets the running <see cref="Resources.Correlator"/>'s options
    /// </summary>
    protected CorrelatorOptions CorrelatorOptions { get; } = correlatorOptions.Value;

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Resources.Correlator"/>
    /// </summary>
    protected IResourceMonitor<Resources.Correlator> Correlator => correlatorAccessor.Correlator;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains current <see cref="CorrelationHandler"/>s
    /// </summary>
    protected ConcurrentDictionary<string, CorrelationHandler> Correlators { get; } = [];

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
        this.Correlator!.Select(b => b.Resource.Spec.Selector).SubscribeAsync(this.OnResourceSelectorChangedAsync, cancellationToken: cancellationToken);
        await this.OnResourceSelectorChangedAsync(this.Correlator!.Resource.Spec.Selector).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override Task ReconcileAsync(CancellationToken cancellationToken = default)
    {
        this.Options.LabelSelectors = this.Correlator?.Resource.Spec.Selector?.Select(s => new LabelSelector(s.Key, LabelSelectionOperator.Equals, s.Value)).ToList();
        return base.ReconcileAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new <see cref="CorrelationHandler"/> for the specified workflow
    /// </summary>
    /// <param name="correlation">The resource of the workflow to create a new scheduler for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="CorrelationHandler"/></returns>
    protected virtual async Task<CorrelationHandler> CreateCorrelatorAsync(Correlation correlation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(correlation);
        var expressionEvaluator = this.ExpressionEvaluatorProvider.GetEvaluator(correlation.Spec.Expressions.Language) ?? throw new NullReferenceException($"Failed to find a registered expression evaluator that supports the configured language '{correlation.Spec.Expressions.Language}'");
        var correlationMonitor = new ResourceMonitor<Correlation>(this.Watch ?? await this.Repository.WatchAsync<Correlation>(cancellationToken: cancellationToken).ConfigureAwait(false), correlation, this.Watch != null);
        var correlator = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, correlationMonitor, expressionEvaluator);
        if (!this.Correlators.TryAdd(correlation.GetQualifiedName(), correlator)) await correlator.DisposeAsync().ConfigureAwait(false);
        return correlator;
    }

    /// <summary>
    /// Attempts to claim the specified <see cref="Correlation"/>
    /// </summary>
    /// <param name="correlation">The <see cref="Correlation"/> to try to claim</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be claimed</returns>
    protected virtual async Task<bool> TryClaimAsync(Correlation correlation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(correlation);
        if (this.Correlator == null) throw new Exception("The controller must be started before attempting any operation");
        if (correlation.Metadata.Labels != null && correlation.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Correlator, out var correlatorQualifiedName) && correlatorQualifiedName == this.Correlator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = correlation.Clone();
            correlation.Metadata.Labels ??= new Dictionary<string, string>();
            correlation.Metadata.Labels[SynapseDefaults.Resources.Labels.Correlator] = this.Correlator.Resource.GetQualifiedName();
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, correlation);
            correlation = await this.Repository.PatchAsync<Correlation>(new(PatchType.JsonPatch, patch), correlation.GetName(), correlation.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
            if (correlation.Status?.Phase == null || correlation.Status.Phase == CorrelationStatusPhase.Pending)
            {
                originalResource = correlation.Clone();
                correlation.Status ??= new();
                correlation.Status.Phase = CorrelationStatusPhase.Active;
                correlation.Status.LastModified = DateTimeOffset.Now;
                originalResource = await this.Repository.PatchStatusAsync<Correlation>(new(PatchType.JsonPatch, JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, correlation)), correlation.GetName(), correlation.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
            }
            return true;
        }
        catch (ConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to release the specified <see cref="Correlation"/>
    /// </summary>
    /// <param name="correlation">The <see cref="Correlation"/> to try to release</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be released</returns>
    protected virtual async Task<bool> TryReleaseAsync(Correlation correlation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(correlation);
        if (correlation.Metadata.Labels != null && correlation.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Correlator, out var correlatorQualifiedName) && correlatorQualifiedName == this.Correlator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = correlation.Clone();
            if (correlation.Status?.Phase == null || correlation.Status.Phase == CorrelationStatusPhase.Active)
            {
                correlation.Status ??= new();
                correlation.Status.Phase = CorrelationStatusPhase.Pending;
                correlation.Status.LastModified = DateTimeOffset.Now;
                originalResource = await this.Repository.PatchStatusAsync<Correlation>(new(PatchType.JsonPatch, JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, correlation)), correlation.GetName(), correlation.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
                correlation = originalResource.Clone()!;
            }
            correlation.Metadata.Labels ??= new Dictionary<string, string>();
            correlation.Metadata.Labels.Remove(SynapseDefaults.Resources.Labels.Correlator);
            if (correlation.Metadata.Labels.Count < 1) correlation.Metadata.Labels = null;
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, correlation);
            correlation = await this.Repository.PatchAsync<Correlation>(new(PatchType.JsonPatch, patch), correlation.GetName(), correlation.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
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
        await foreach (var resource in this.Repository.GetAllAsync<Correlation>(labelSelectors: [new LabelSelector(SynapseDefaults.Resources.Labels.Correlator, LabelSelectionOperator.Equals, [this.Correlator.Resource.GetQualifiedName()])], cancellationToken: cancellationToken))
        {
            await this.TryReleaseAsync(resource, cancellationToken).ConfigureAwait(false);
        }
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceCreatedAsync(Correlation correlation, CancellationToken cancellationToken = default)
    {
        await base.OnResourceCreatedAsync(correlation, cancellationToken).ConfigureAwait(false);
        if (!await this.TryClaimAsync(correlation, cancellationToken).ConfigureAwait(false)) return;
        var correlator = await this.CreateCorrelatorAsync(correlation,cancellationToken).ConfigureAwait(false);
        await correlator.HandleAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(Correlation correlation, CancellationToken cancellationToken = default)
    {
        await base.OnResourceDeletedAsync(correlation, cancellationToken).ConfigureAwait(false);
        if (this.Correlators.TryRemove(correlation.GetQualifiedName(), out var handler)) await handler.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles changes to the current correlator's subscription selector
    /// </summary>
    /// <param name="selector">A key/value mapping of the labels both correlations and correlation instances to select must define</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceSelectorChangedAsync(IDictionary<string, string>? selector) => this.ReconcileAsync(this.CancellationTokenSource.Token);

}
