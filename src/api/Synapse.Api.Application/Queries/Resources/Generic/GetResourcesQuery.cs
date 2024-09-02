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

namespace Synapse.Api.Application.Queries.Resources.Generic;

/// <summary>
/// Represents the <see cref="IQuery{TResult}"/> used to retrieves <see cref="IResource"/>s of the specified type
/// </summary>
/// <remarks>
/// Initializes a new <see cref="GetResourcesQuery{TResource}"/>
/// </remarks>
/// <param name="namespace">The namespace the <see cref="IResource"/>s to get belong to, if any</param>
/// <param name="labelSelectors">A <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="IResource"/>s by</param>
public class GetResourcesQuery<TResource>(string? @namespace, IEnumerable<LabelSelector>? labelSelectors)
    : Query<IAsyncEnumerable<TResource>>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Gets the namespace the <see cref="IResource"/>s to get belong to, if any
    /// </summary>
    public string? Namespace { get; } = @namespace;

    /// <summary>
    /// Gets a <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="IResource"/>s by
    /// </summary>
    public IEnumerable<LabelSelector>? LabelSelectors { get; } = labelSelectors;

}

/// <summary>
/// Represents the service used to handle <see cref="GetResourcesQuery{TResource}"/> instances
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to get</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class GetResourcesQueryHandler<TResource>(IResourceRepository repository)
    : IQueryHandler<GetResourcesQuery<TResource>, IAsyncEnumerable<TResource>>
    where TResource : class, IResource, new()
{


    /// <inheritdoc/>
    public virtual Task<IOperationResult<IAsyncEnumerable<TResource>>> HandleAsync(GetResourcesQuery<TResource> query, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Ok(repository.GetAllAsync<TResource>(query.Namespace, query.LabelSelectors, cancellationToken)));
    }

}