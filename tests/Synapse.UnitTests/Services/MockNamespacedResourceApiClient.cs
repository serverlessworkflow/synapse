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

using Neuroglia.Data;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Synapse.Api.Client.Services;
using System.Runtime.CompilerServices;

namespace Synapse.UnitTests.Services;

internal class MockNamespacedResourceApiClient<TResource>(IResourceRepository resources)
    : INamespacedResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

    protected IResourceRepository Resources { get; } = resources;

    public Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken = default) => this.Resources.AddAsync(resource, false, cancellationToken);

    public Task<TResource> GetAsync(string name, string @namespace, CancellationToken cancellationToken = default) => this.Resources.GetAsync<TResource>(name, @namespace, cancellationToken)!;

    public async Task<ResourceDefinition> GetDefinitionAsync(CancellationToken cancellationToken = default) => ((ResourceDefinition)(await this.Resources.GetDefinitionAsync<TResource>(cancellationToken))!)!;

    public Task<IAsyncEnumerable<TResource>> ListAsync(string? @namespace, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default) => Task.FromResult(this.Resources.GetAllAsync<TResource>(@namespace, labelSelectors, cancellationToken: cancellationToken)!);

    public async IAsyncEnumerable<IResourceWatchEvent<TResource>> WatchAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        await foreach (var e in (await this.Resources.WatchAsync<TResource>(@namespace, labelSelectors, cancellationToken).ConfigureAwait(false)).ToAsyncEnumerable()) yield return e;
    }

    public async IAsyncEnumerable<IResourceWatchEvent<TResource>> MonitorAsync(string name, string @namespace, [EnumeratorCancellation] CancellationToken cancellationToken = default) 
    {
        await foreach (var e in (await this.Resources.MonitorAsync<TResource>(name, @namespace, false, cancellationToken).ConfigureAwait(false)).ToAsyncEnumerable()) yield return e;
    }

    public Task<TResource> PatchAsync(string name, string @namespace, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default) => this.Resources.PatchAsync<TResource>(patch, name, @namespace, resourceVersion, false, cancellationToken);

    public Task<TResource> PatchStatusAsync(string name, string @namespace, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default) => this.Resources.PatchStatusAsync<TResource>(patch, name, @namespace, resourceVersion, false, cancellationToken);

    public Task<TResource> ReplaceAsync(TResource resource, CancellationToken cancellationToken = default) => this.Resources.ReplaceAsync(resource, false, cancellationToken);

    public Task<TResource> ReplaceStatusAsync(TResource resource, CancellationToken cancellationToken = default) => this.Resources.ReplaceStatusAsync(resource, false, cancellationToken);

    public Task DeleteAsync(string name, string @namespace, CancellationToken cancellationToken = default) => this.Resources.RemoveAsync<TResource>(name, @namespace, false, cancellationToken);

}
