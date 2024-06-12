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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage namespaced <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public interface INamespacedResourceApiClient<TResource>
    : IResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Lists <see cref="IResource"/>s
    /// </summary>
    /// <param name="namespace">The namespace the <see cref="IResource"/>s to list belong to</param>
    /// <param name="labelSelectors">An <see cref="IEnumerable{T}"/> containing the <see cref="LabelSelector"/>s used to select the <see cref="IResource"/>s to list by, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to asynchronously enumerate resulting <see cref="IResource"/>s</returns>
    Task<IAsyncEnumerable<TResource>> ListAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Watches matching resources
    /// </summary>
    /// <param name="namespace">The namespace the resources to watch belong to</param>
    /// <param name="labelSelectors">Defines the expected labels, if any, of the resources to watch</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to asynchronously enumerate resulting <see cref="IResourceWatchEvent"/>s</returns>
    Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> WatchAsync(string? @namespace = null, IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitors the resource with the specified name
    /// </summary>
    /// <param name="name">The name of the resource to monitor</param>
    /// <param name="namespace">The namespace the resource to monitor belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to asynchronously enumerate resulting <see cref="IResourceWatchEvent"/>s</returns>
    Task<IAsyncEnumerable<IResourceWatchEvent<TResource>>> MonitorAsync(string name, string @namespace, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the resource with the specified name
    /// </summary>
    /// <param name="name">The name of the resource to get</param>
    /// <param name="namespace">The namespace the resource to get belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The resource with the specified name</returns>
    Task<TResource> GetAsync(string name, string @namespace, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches the specified resource
    /// </summary>
    /// <param name="name">The name of the resource to patch</param>
    /// <param name="namespace">The namespace the resource to patch belongs to</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="resourceVersion">The expected resource version, if any, used for optimistic concurrency</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The patched resource</returns>
    Task<TResource> PatchAsync(string name, string @namespace, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches the specified resource's status
    /// </summary>
    /// <param name="name">The name of the resource to patch the status of</param>
    /// <param name="namespace">The namespace the resource to patch the status of belongs to</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="resourceVersion">The expected resource version, if any, used for optimistic concurrency</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The patched resource</returns>
    Task<TResource> PatchStatusAsync(string name, string @namespace, Patch patch, string? resourceVersion = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified resource
    /// </summary>
    /// <param name="name">The name of the resource to delete</param>
    /// <param name="namespace">The namespace the resource to delete belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task DeleteAsync(string name, string @namespace, CancellationToken cancellationToken = default);

}
