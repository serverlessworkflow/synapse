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

namespace Synapse.Api.Application.Queries.Resources;

/// <summary>
/// Represents the <see cref="IQuery{TResult}"/> used to list all <see cref="IResource"/>s
/// </summary>
public class ListResourcesQuery
    : Query<ICollection>
{

    /// <summary>
    /// Initializes a new <see cref="ListResourcesQuery"/>
    /// </summary>
    /// <param name="group">The API group the resources to list belongs to</param>
    /// <param name="version">The version of the resources to list</param>
    /// <param name="plural">The plural name of the type of resources to list</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/>s to list belong to, if any</param>
    /// <param name="labelSelectors">A <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="IResource"/>s by</param>
    /// <param name="maxResults">The maximum amount of results to list</param>
    /// <param name="continuationToken">The token, if any, used to continue enumerating the results</param>
    public ListResourcesQuery(string group, string version, string plural, string? @namespace, List<LabelSelector>? labelSelectors, ulong? maxResults, string? continuationToken)
    {
        if (string.IsNullOrWhiteSpace(group)) throw new ArgumentNullException(nameof(group));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));
        if (string.IsNullOrWhiteSpace(plural)) throw new ArgumentNullException(nameof(plural));
        this.Group = group;
        this.Version = version;
        this.Plural = plural;
        this.Namespace = @namespace;
        this.LabelSelectors = labelSelectors;
        this.MaxResults = maxResults;
        this.ContinuationToken = continuationToken;
    }

    /// <summary>
    /// Gets the API group the resources to list belongs to
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the version of the resources to list
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the plural name of the type of resources to list
    /// </summary>
    public string Plural { get; }

    /// <summary>
    /// Gets the namespace the <see cref="IResource"/>s to list belong to, if any
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets a <see cref="List{T}"/> containing the <see cref="LabelSelector"/>s used to filter <see cref="IResource"/>s by
    /// </summary>
    public List<LabelSelector>? LabelSelectors { get; }

    /// <summary>
    /// Gets the maximum amount of results to list
    /// </summary>
    public ulong? MaxResults { get; }

    /// <summary>
    /// Gets the token, if any, used to continue enumerating the results
    /// </summary>
    public string? ContinuationToken { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="ListResourcesQuery"/> instances
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class ListResourcesQueryHandler(IResourceRepository repository)
    : IQueryHandler<ListResourcesQuery, ICollection>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<ICollection>> HandleAsync(ListResourcesQuery query, CancellationToken cancellationToken)
    {
        return this.Ok(await repository.ListAsync(query.Group, query.Version, query.Plural, query.Namespace, query.LabelSelectors, query.MaxResults, query.ContinuationToken, cancellationToken).ConfigureAwait(false));
    }

}
