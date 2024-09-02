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
/// Represents the <see cref="IQuery{TResult}"/> used to get an existing <see cref="IResource"/>
/// </summary>
public class GetResourceQuery
    : Query<IResource>
{

    /// <summary>
    /// Initializes a new <see cref="GetResourceQuery"/>
    /// </summary>
    /// <param name="group">The API group the resource to get belongs to</param>
    /// <param name="version">The version of the resource to get</param>
    /// <param name="plural">The plural name of the type of resource to get</param>
    /// <param name="name">The name of the <see cref="IResource"/> to get</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/> to get belongs to</param>
    public GetResourceQuery(string group, string version, string plural, string name, string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(group)) throw new ArgumentNullException(nameof(group));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));
        if (string.IsNullOrWhiteSpace(plural)) throw new ArgumentNullException(nameof(plural));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Group = group;
        this.Version = version;
        this.Plural = plural;
        this.Name = name;
        this.Namespace = @namespace;
    }

    /// <summary>
    /// Gets the API group the resource to get belongs to
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the version of the resource to get
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the plural name of the type of resource to get
    /// </summary>
    public string Plural { get; }

    /// <summary>
    /// Gets the name of the <see cref="IResource"/> to get
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the namespace the <see cref="IResource"/> to get belongs to
    /// </summary>
    public string? Namespace { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="GetResourceQuery"/>s
/// </summary>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class GetResourceQueryHandler(IResourceRepository repository)
    : IQueryHandler<GetResourceQuery, IResource>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IResource>> HandleAsync(GetResourceQuery query, CancellationToken cancellationToken)
    {
        return this.Ok(await repository.GetAsync(query.Group, query.Version, query.Plural, query.Name, query.Namespace, cancellationToken).ConfigureAwait(false));
    }

}
