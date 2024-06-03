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

namespace Synapse.Correlator.Services;

/// <summary>
/// Represents the service used to manage <see cref="Correlation"/> resources
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="controllerOptions">The service used to access the current <see cref="IOptions{TOptions}"/></param>
/// <param name="resources">The service used to manage <see cref="IResource"/>s</param>
/// <param name="correlatorOptions">The current <see cref="Configuration.CorrelatorOptions"/></param>
/// <param name="correlatorAccessor">The service used to access the current <see cref="Resources.Correlator"/></param>
public class CorrelationController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<Correlation>> controllerOptions, IResourceRepository resources, IOptions<CorrelatorOptions> correlatorOptions, ICorrelatorController correlatorAccessor)
    : ResourceController<Correlation>(loggerFactory, controllerOptions, resources)
{

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the running <see cref="Resources.Correlator"/>'s options
    /// </summary>
    protected CorrelatorOptions CorrelatorOptions { get; } = correlatorOptions.Value;

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Resources.Correlator"/>
    /// </summary>
    protected IResourceMonitor<Resources.Correlator> Correlator => correlatorAccessor.Correlator;

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
        foreach (var correlationInstance in this.Resources.Values.ToList()) await this.OnResourceCreatedAsync(correlationInstance, cancellationToken).ConfigureAwait(false);
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
    /// Attempts to claim the specified <see cref="Correlation"/>
    /// </summary>
    /// <param name="resource">The <see cref="Correlation"/> to try to claim</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be claimed</returns>
    protected virtual async Task<bool> TryClaimAsync(Correlation resource, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (this.Correlator == null) throw new Exception("The controller must be started before attempting any operation");
        if (resource.Metadata.Labels != null && resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Correlator, out var correlatorQualifiedName) && correlatorQualifiedName == this.Correlator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels[SynapseDefaults.Resources.Labels.Correlator] = this.Correlator.Resource.GetQualifiedName();
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<Correlation>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
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
    /// <param name="resource">The <see cref="Correlation"/> to try to release</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A boolean indicating whether or not the instance could be released</returns>
    protected virtual async Task<bool> TryReleaseAsync(Correlation resource, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.Metadata.Labels != null && resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Correlator, out var correlatorQualifiedName) && correlatorQualifiedName == this.Correlator.Resource.GetQualifiedName()) return true;
        try
        {
            var originalResource = resource.Clone();
            resource.Metadata.Labels ??= new Dictionary<string, string>();
            resource.Metadata.Labels.Remove(SynapseDefaults.Resources.Labels.Correlator);
            if (resource.Metadata.Labels.Count < 1) resource.Metadata.Labels = null;
            var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, resource);
            resource = await this.Repository.PatchAsync<Correlation>(new(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, cancellationToken).ConfigureAwait(false);
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
    protected override async Task OnResourceCreatedAsync(Correlation resource, CancellationToken cancellationToken = default)
    {
        await base.OnResourceCreatedAsync(resource, cancellationToken).ConfigureAwait(false);
        if (!await this.TryClaimAsync(resource, cancellationToken).ConfigureAwait(false)) return;

    }

    /// <inheritdoc/>
    protected override async Task OnResourceUpdatedAsync(Correlation resource, CancellationToken cancellationToken = default)
    {
        await base.OnResourceUpdatedAsync(resource, cancellationToken).ConfigureAwait(false);
        if (resource.Metadata.Labels == null || !resource.Metadata.Labels.TryGetValue(SynapseDefaults.Resources.Labels.Correlator, out _)) if (!await this.TryClaimAsync(resource, cancellationToken).ConfigureAwait(false)) return;
        if (resource.Metadata.Labels?[SynapseDefaults.Resources.Labels.Correlator] != this.Correlator.Resource.GetQualifiedName()) return;
        
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(Correlation resource, CancellationToken cancellationToken = default)
    {
        await base.OnResourceDeletedAsync(resource, cancellationToken).ConfigureAwait(false);
        
    }

    /// <summary>
    /// Handles changes to the current correlator's subscription selector
    /// </summary>
    /// <param name="selector">A key/value mapping of the labels both correlations and correlation instances to select must define</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceSelectorChangedAsync(IDictionary<string, string>? selector) => this.ReconcileAsync(this.CancellationTokenSource.Token);

}
