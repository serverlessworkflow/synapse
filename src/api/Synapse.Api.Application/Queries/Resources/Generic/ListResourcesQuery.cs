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
/// Represents the <see cref="IQuery{TResult}"/> used to list all <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to list</typeparam>
/// <remarks>
/// Initializes a new <see cref="ListResourcesQuery{TResource}"/>
/// </remarks>
/// <param name="namespace">The namespace the <see cref="IResource"/>s to get belong to, if any</param>
/// <param name="labelSelectors">A <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="IResource"/>s by</param>
/// <param name="maxResults">The maximum amount of results to list</param>
/// <param name="continuationToken">The token, if any, used to continue enumerating the results</param>
public class ListResourcesQuery<TResource>(string? @namespace, IEnumerable<LabelSelector>? labelSelectors, ulong? maxResults, string? continuationToken)
    : Query<Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<TResource>>
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

    /// <summary>
    /// Gets the maximum amount of results to list
    /// </summary>
    public ulong? MaxResults { get; } = maxResults;

    /// <summary>
    /// Gets the token, if any, used to continue enumerating the results
    /// </summary>
    public string? ContinuationToken { get; } = continuationToken;

}

/// <summary>
/// Represents the service used to handle <see cref="ListResourcesQuery{TResource}"/> instances
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to list</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class ListResourcesQueryHandler<TResource>(IResourceRepository repository)
    : IQueryHandler<ListResourcesQuery<TResource>, Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<TResource>>
    where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<Neuroglia.Data.Infrastructure.ResourceOriented.ICollection<TResource>>> HandleAsync(ListResourcesQuery<TResource> query, CancellationToken cancellationToken)
    {
        return this.Ok(await repository.ListAsync<TResource>(query.Namespace, query.LabelSelectors, query.MaxResults, query.ContinuationToken, cancellationToken).ConfigureAwait(false));
    }

}
