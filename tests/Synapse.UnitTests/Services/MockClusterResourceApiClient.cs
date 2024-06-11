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

namespace Synapse.UnitTests.Services;

internal class MockClusterResourceApiClient<TResource>(IResourceRepository resources)
    : IClusterResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

    public Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken = default) => resources.AddAsync(resource, false, cancellationToken);

    public Task<IAsyncEnumerable<TResource>> ListAsync(IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default) => Task.FromResult(resources.GetAllAsync<TResource>(null, labelSelectors, cancellationToken)!);
    
    public async Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> WatchAsync(IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default) => (await resources.WatchAsync<TResource>(null!, labelSelectors, cancellationToken).ConfigureAwait(false)).ToAsyncEnumerable();
    
    public async Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> MonitorAsync(string name, CancellationToken cancellationToken = default) => (await resources.MonitorAsync<TResource>(name, null!, false, cancellationToken).ConfigureAwait(false)).ToAsyncEnumerable();
    
    public Task<TResource> GetAsync(string name, CancellationToken cancellationToken = default) => resources.GetAsync<TResource>(name, null, cancellationToken)!;
    
    public async Task<ResourceDefinition> GetDefinitionAsync(CancellationToken cancellationToken = default) => ((ResourceDefinition)(await resources.GetDefinitionAsync<TResource>(cancellationToken))!)!;

    public Task<TResource> PatchAsync(string name, Patch patch, CancellationToken cancellationToken = default) => resources.PatchAsync<TResource>(patch, name, null, false, cancellationToken);

    public Task<TResource> PatchStatusAsync(string name, Patch patch, CancellationToken cancellationToken = default) => resources.PatchStatusAsync<TResource>(patch, name, null, false, cancellationToken);

    public Task<TResource> ReplaceAsync(TResource resource, CancellationToken cancellationToken = default) => resources.ReplaceAsync(resource, false, cancellationToken);

    public Task<TResource> ReplaceStatusAsync(TResource resource, CancellationToken cancellationToken = default) => resources.ReplaceStatusAsync(resource, false, cancellationToken);

    public Task DeleteAsync(string name, CancellationToken cancellationToken = default) => resources.RemoveAsync<TResource>(name, null, false, cancellationToken);

}
