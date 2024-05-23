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