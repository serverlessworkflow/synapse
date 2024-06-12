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

namespace Synapse.Correlator.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ICorrelatorController"/> interface
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
/// <param name="options">The current <see cref="CorrelatorOptions"/></param>
public class CorrelatorController(IResourceRepository repository, IOptionsMonitor<CorrelatorOptions> options)
    : ICorrelatorController
{

    /// <summary>
    /// Gets the service used to manage <see cref="IResource"/>s
    /// </summary>
    protected IResourceRepository Repository { get; } = repository;

    /// <summary>
    /// Gets the current <see cref="CorrelatorOptions"/>
    /// </summary>
    protected CorrelatorOptions Options => options.CurrentValue;

    /// <inheritdoc/>
    public IResourceMonitor<Resources.Correlator> Correlator { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        Resources.Correlator? correlator = null;
        try
        {
            correlator = await this.Repository.GetAsync<Resources.Correlator>(this.Options.Name, this.Options.Namespace, cancellationToken).ConfigureAwait(false);
        }
        catch (ProblemDetailsException ex) when (ex.Problem.Status == (int)HttpStatusCode.NotFound) { }
        finally
        {
            if (correlator == null)
            {
                correlator = new Resources.Correlator(new ResourceMetadata(this.Options.Name, this.Options.Namespace), new CorrelatorSpec() 
                { 
                    
                });
                await this.Repository.AddAsync(correlator, false, cancellationToken).ConfigureAwait(false);
            }
            this.Correlator = await this.Repository.MonitorAsync<Resources.Correlator>(this.Options.Name, this.Options.Namespace, false, cancellationToken).ConfigureAwait(false);
            await this.SetCorrelatorStatusPhaseAsync(CorrelatorStatusPhase.Running, cancellationToken).ConfigureAwait(false);
            this.Correlator.Where(e => e.Type == ResourceWatchEventType.Updated).Select(o => o.Resource.Spec).DistinctUntilChanged().Subscribe(_ => this.OnCorrelatorSpecChanged(), token: cancellationToken);
            this.OnCorrelatorSpecChanged();
        }
    }

    /// <summary>
    /// Sets the <see cref="Resources.Correlator"/>'s status phase
    /// </summary>
    /// <param name="phase">The <see cref="Resources.Correlator"/>'s status phase</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task SetCorrelatorStatusPhaseAsync(string phase, CancellationToken cancellationToken = default)
    {
        if (this.Correlator.Resource.Status?.Phase == phase) return;
        var updatedResource = this.Correlator.Resource.Clone()!;
        var originalResource = this.Correlator.Resource.Clone()!;
        updatedResource.Status ??= new();
        updatedResource.Status.Phase = phase;
        var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, updatedResource);
        await this.Repository.PatchStatusAsync<Resources.Correlator>(new(PatchType.JsonPatch, patch), updatedResource.GetName(), updatedResource.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles changes to the operator spec, and update the operator options accordingly
    /// </summary>
    protected virtual void OnCorrelatorSpecChanged()
    {
        
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.SetCorrelatorStatusPhaseAsync(CorrelatorStatusPhase.Stopped, cancellationToken).ConfigureAwait(false);
    }

}