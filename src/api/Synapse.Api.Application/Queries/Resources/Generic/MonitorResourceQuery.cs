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
public class MonitorResourceQuery<TResource>
    : Query<IAsyncEnumerable<IResourceWatchEvent<TResource>>>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Initializes a new <see cref="MonitorResourceQuery{TResource}"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="IResource"/> to monitor</param>
    /// <param name="namespace">The namespace the <see cref="IResource"/> to monitor belongs to, if any</param>
    public MonitorResourceQuery(string name, string? @namespace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this.Name = name;
        this.Namespace = @namespace;
    }

    /// <summary>
    /// Gets the name of the <see cref="IResource"/>s to monitor
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the namespace the <see cref="IResource"/>s to get belongs to, if any
    /// </summary>
    public string? Namespace { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="MonitorResourceQuery{TResource}"/> instances
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to monitor</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class MonitorResourceQueryHandler<TResource>(IResourceRepository repository)
    : IQueryHandler<MonitorResourceQuery<TResource>, IAsyncEnumerable<IResourceWatchEvent<TResource>>>
    where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IAsyncEnumerable<IResourceWatchEvent<TResource>>>> HandleAsync(MonitorResourceQuery<TResource> query, CancellationToken cancellationToken)
    {
        return this.Ok((await repository.MonitorAsync<TResource>(query.Name, query.Namespace, false, cancellationToken).ConfigureAwait(false)).ToAsyncEnumerable());
    }

}