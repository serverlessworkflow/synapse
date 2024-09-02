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
/// Represents the <see cref="IQuery{TResult}"/> used to get the definition of the specified <see cref="IResource"/> type
/// </summary>
/// <typeparam name="TResource">The type of the <see cref="IResource"/> to get the definition of</typeparam>
public class GetResourceDefinitionQuery<TResource>
    : Query<IResourceDefinition>
    where TResource : class, IResource, new()
{



}

/// <summary>
/// Represents the service used to handle <see cref="GetResourceDefinitionQuery{TResource}"/>s
/// </summary>
/// <typeparam name="TResource">The type of the <see cref="IResource"/> to replace</typeparam>
/// <param name="repository">The service used to manage <see cref="IResource"/>s</param>
public class GetResourceDefinitionQueryHandler<TResource>(IResourceRepository repository)
    : IQueryHandler<GetResourceDefinitionQuery<TResource>, IResourceDefinition>
     where TResource : class, IResource, new()
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IResourceDefinition>> HandleAsync(GetResourceDefinitionQuery<TResource> query, CancellationToken cancellationToken)
    {
        var resourceDefinition = await repository.GetDefinitionAsync<TResource>(cancellationToken).ConfigureAwait(false);
        return this.Ok(resourceDefinition);
    }

}