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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public interface IResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Creates a new resource
    /// </summary>
    /// <param name="resource">The resource to create</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The new resource</returns>
    Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the definition of the resources managed by the API
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The definition of the resources managed by the API</returns>
    Task<ResourceDefinition> GetDefinitionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing resource
    /// </summary>
    /// <param name="resource">The resource to replace</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The replaced resource</returns>
    Task<TResource> ReplaceAsync(TResource resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the status of an existing resource
    /// </summary>
    /// <param name="resource">The resource to replace the status of</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The replaced resource</returns>
    Task<TResource> ReplaceStatusAsync(TResource resource, CancellationToken cancellationToken = default);

}
