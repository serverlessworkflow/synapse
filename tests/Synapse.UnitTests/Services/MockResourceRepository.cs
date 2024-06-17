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
using System.Runtime.CompilerServices;

namespace Synapse.UnitTests.Services;

internal class MockResourceRepository
    : IResourceRepository
{

    public Task<IResource> AddAsync(IResource resource, string group, string version, string plural, bool dryRun = false, CancellationToken cancellationToken = default) => Task.FromResult(resource);

    public async IAsyncEnumerable<IResource> GetAllAsync(string group, string version, string plural, string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task<IResource?> GetAsync(string group, string version, string plural, string name, string? @namespace = null, CancellationToken cancellationToken = default) => Task.FromResult((IResource?)null);

    public Task<ICollection> ListAsync(string group, string version, string plural, string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, ulong? maxResults = null, string? continuationToken = null, CancellationToken cancellationToken = default) => Task.FromResult((ICollection)new Collection());

    public Task<IResource> PatchAsync(Patch patch, string group, string version, string plural, string name, string? @namespace = null, string? resourceVersion = null, bool dryRun = false, CancellationToken cancellationToken = default) => Task.FromResult((IResource)new Resource(new ResourceDefinitionInfo(group, version, plural, plural)) 
    { 
        Metadata = new() 
        {  
            Name = name,
            Namespace = @namespace
        }
    });

    public Task<IResource> PatchSubResourceAsync(Patch patch, string group, string version, string plural, string name, string subResource, string? @namespace = null, string? resourceVersion = null, bool dryRun = false, CancellationToken cancellationToken = default) => Task.FromResult((IResource)new Resource(new ResourceDefinitionInfo(group, version, plural, plural))
    {
        Metadata = new()
        {
            Name = name,
            Namespace = @namespace
        }
    });

    public Task<IResource> RemoveAsync(string group, string version, string plural, string name, string? @namespace = null, bool dryRun = false, CancellationToken cancellationToken = default) => Task.FromResult((IResource)new Resource(new ResourceDefinitionInfo(group, version, plural, plural))
    {
        Metadata = new()
        {
            Name = name,
            Namespace = @namespace
        }
    });

    public Task<IResource> ReplaceAsync(IResource resource, string group, string version, string plural, bool dryRun = false, CancellationToken cancellationToken = default) => Task.FromResult(resource);

    public Task<IResource> ReplaceSubResourceAsync(IResource resource, string group, string version, string plural, string subResource, bool dryRun = false, CancellationToken cancellationToken = default) => Task.FromResult(resource);

    public Task<IResourceWatch> WatchAsync(string group, string version, string plural, string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

}
