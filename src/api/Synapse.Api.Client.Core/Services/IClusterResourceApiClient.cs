using Neuroglia.Data;

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage cluster <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
public interface IClusterResourceApiClient<TResource>
    : IResourceApiClient<TResource>
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Lists <see cref="IResource"/>s
    /// </summary>
    /// <param name="labelSelectors">An <see cref="IEnumerable{T}"/> containing the <see cref="LabelSelector"/>s used to select the <see cref="IResource"/>s to list by, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to asynchronously enumerate resulting <see cref="IResource"/>s</returns>
    Task<IAsyncEnumerable<TResource>> ListAsync(IEnumerable<LabelSelector>? labelSelectors = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the resource with the specified name
    /// </summary>
    /// <param name="name">The name of the resource to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The resource with the specified name</returns>
    Task<TResource> GetAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches the specified resource
    /// </summary>
    /// <param name="name">The name of the resource to patch</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The patched resource</returns>
    Task<TResource> PatchAsync(string name, Patch patch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches the specified resource's status
    /// </summary>
    /// <param name="name">The name of the resource to patch the status of</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The patched resource</returns>
    Task<TResource> PatchStatusAsync(string name, Patch patch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified resource
    /// </summary>
    /// <param name="name">The name of the resource to delete</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task DeleteAsync(string name, CancellationToken cancellationToken = default);

}
