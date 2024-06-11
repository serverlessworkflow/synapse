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
/// Represents the <see cref="IQuery{TResult}"/> used to get an existing <see cref="IResource"/>
/// </summary>
/// <typeparam name="TResource">The type of the <see cref="IResource"/> to get</typeparam>
public class GetResourceQuery<TResource>
    : Query<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Initializes a new <see cref="GetResourceQuery{TResource}"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="IResource"/> to get</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/> to get belongs to</param>
    public GetResourceQuery(string name, string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Namespace = @namespace;
    }

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
/// Represents the service used to handle <see cref="GetResourceQuery{TResource}"/>s
/// </summary>
/// <typeparam name="TResource">The type of the <see cref="IResource"/> to replace</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class GetResourceQueryHandler<TResource>(IResourceRepository repository)
    : IQueryHandler<GetResourceQuery<TResource>, TResource>
     where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<TResource>> HandleAsync(GetResourceQuery<TResource> query, CancellationToken cancellationToken)
    {
        var resource = await repository.GetAsync<TResource>(query.Name, query.Namespace, cancellationToken).ConfigureAwait(false) ?? throw new ProblemDetailsException(ResourceProblemDetails.ResourceNotFound(new ResourceReference<TResource>(query.Name, query.Namespace)));
        return this.Ok(resource);
    }

}
