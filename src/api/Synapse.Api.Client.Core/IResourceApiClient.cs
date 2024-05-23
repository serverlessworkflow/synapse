using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Api;

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
