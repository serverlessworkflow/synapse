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

namespace Synapse.Operator.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IOperatorController"/> interface
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
/// <param name="options">The current <see cref="OperatorOptions"/></param>
/// <param name="runnerOptions">The current <see cref="RunnerDefinition"/></param>
public class OperatorController(IResourceRepository repository, IOptionsMonitor<OperatorOptions> options, IOptionsMonitor<RunnerDefinition> runnerOptions)
    : IOperatorController
{

    /// <summary>
    /// Gets the service used to manage <see cref="IResource"/>s
    /// </summary>
    protected IResourceRepository Repository { get; } = repository;

    /// <summary>
    /// Gets the current <see cref="OperatorOptions"/>
    /// </summary>
    protected OperatorOptions Options => options.CurrentValue;

    /// <summary>
    /// Gets the current <see cref="RunnerDefinition"/>
    /// </summary>
    protected RunnerDefinition RunnerOptions => runnerOptions.CurrentValue;

    /// <inheritdoc/>
    public IResourceMonitor<Resources.Operator> Operator { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        Resources.Operator? @operator = null;
        try
        {
            @operator = await this.Repository.GetAsync<Resources.Operator>(this.Options.Name, this.Options.Namespace, cancellationToken).ConfigureAwait(false);
        }
        catch (ProblemDetailsException ex) when (ex.Problem.Status == (int)HttpStatusCode.NotFound) { }
        finally
        {
            if (@operator == null)
            {
                @operator = new Resources.Operator(new ResourceMetadata(this.Options.Name, this.Options.Namespace), new OperatorSpec() 
                { 
                    Runner = this.Options.Runner
                });
                await this.Repository.AddAsync(@operator, false, cancellationToken).ConfigureAwait(false);
            }
            this.Operator = await this.Repository.MonitorAsync<Resources.Operator>(this.Options.Name, this.Options.Namespace, false, cancellationToken).ConfigureAwait(false);
            await this.SetOperatorStatusPhaseAsync(OperatorStatusPhase.Running, cancellationToken).ConfigureAwait(false);
            this.Operator.Where(e => e.Type == ResourceWatchEventType.Updated).Select(o => o.Resource.Spec).DistinctUntilChanged().Subscribe(_ => this.OnOperatorSpecChanged(), token: cancellationToken);
            this.OnOperatorSpecChanged();
        }
    }

    /// <summary>
    /// Sets the <see cref="Resources.Operator"/>'s status phase
    /// </summary>
    /// <param name="phase">The <see cref="Resources.Operator"/>'s status phase</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task SetOperatorStatusPhaseAsync(string phase, CancellationToken cancellationToken = default)
    {
        if (this.Operator.Resource.Status?.Phase == phase) return;
        var updatedResource = this.Operator.Resource.Clone()!;
        var originalResource = this.Operator.Resource.Clone()!;
        updatedResource.Status ??= new();
        updatedResource.Status.Phase = phase;
        var patch = JsonPatchUtility.CreateJsonPatchFromDiff(originalResource, updatedResource);
        await this.Repository.PatchStatusAsync<Resources.Operator>(new(PatchType.JsonPatch, patch), updatedResource.GetName(), updatedResource.GetNamespace(), null, false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles changes to the operator spec, and update the operator options accordingly
    /// </summary>
    protected virtual void OnOperatorSpecChanged()
    {
        this.Options.Runner = this.Operator.Resource.Spec.Runner;
        this.RunnerOptions.Api = this.Options.Runner.Api;
        this.RunnerOptions.Runtime = this.Options.Runner.Runtime;
        this.RunnerOptions.Certificates = this.Options.Runner.Certificates;
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.SetOperatorStatusPhaseAsync(OperatorStatusPhase.Stopped, cancellationToken).ConfigureAwait(false);
    }

}