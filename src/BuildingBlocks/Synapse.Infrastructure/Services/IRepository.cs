using Neuroglia.K8s;
using Synapse.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Synapse.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to query and persist <see cref="ICustomResource"/>s
    /// </summary>
    /// <typeparam name="TResource">The type of <see cref="ICustomResource"/> to manage</typeparam>
    public interface IRepository<TResource>
        where TResource : class, ICustomResource, IAggregateRoot, new()
    {

        /// <summary>
        /// Finds the specified <see cref="ICustomResource"/>
        /// </summary>
        /// <param name="name">The unique name of the <see cref="ICustomResource"/> to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="ICustomResource"/> with the specified name, if any</returns>
        Task<TResource> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds the specified <see cref="ICustomResource"/>
        /// </summary>
        /// <param name="name">The unique name of the <see cref="ICustomResource"/> to find</param>
        /// <param name="namespace">The namespace the resource to find belongs to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="ICustomResource"/> with the specified name, if any</returns>
        Task<TResource> FindByNameAsync(string name, string @namespace, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds and persists the specified <see cref="ICustomResource"/>
        /// </summary>
        /// <param name="resource">The <see cref="ICustomResource"/> to add</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly added <see cref="ICustomResource"/></returns>
        Task<TResource> AddAsync(TResource resource, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified <see cref="ICustomResource"/>
        /// </summary>
        /// <param name="resource">The <see cref="ICustomResource"/> to update</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The updated <see cref="ICustomResource"/></returns>
        Task<TResource> UpdateAsync(TResource resource, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the specified <see cref="ICustomResource"/>
        /// </summary>
        /// <param name="resource">The <see cref="ICustomResource"/> to remove</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task RemoveAsync(TResource resource, CancellationToken cancellationToken = default);

        /// <summary>
        /// Filters <see cref="ICustomResource"/>s
        /// </summary>
        /// <param name="labelSelector">The CSV label selectors to use</param>
        /// <param name="fieldSelector">The CSV field selectors to use</param>
        /// <param name="namespace">The namespace the <see cref="ICustomResource"/> to filter belong to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IEnumerable{T}"/> containing the filtered <see cref="ICustomResource"/>s</returns>
        Task<IEnumerable<TResource>> FilterAsync(string labelSelector = null, string fieldSelector = null, string @namespace = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists all <see cref="ICustomResource"/>s
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IEnumerable{T}"/> containing all available <see cref="ICustomResource"/>s</returns>
        Task<IEnumerable<TResource>> ToListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all <see cref="ICustomResource"/>s in the specified namespace
        /// </summary>
        /// <param name="namespace">The namespace to remove all <see cref="ICustomResource"/>s of the specified type from</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task ClearAsync(string @namespace, CancellationToken cancellationToken = default);

    }

}
