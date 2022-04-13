namespace Synapse.Worker.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage secrets
    /// </summary>
    public interface ISecretManager
    {

        /// <summary>
        /// Gets all available secrets
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IDictionary{TKey, TValue}"/> that contains the key/value mappings of all available secrets</returns>
        Task<IDictionary<string, object>> GetSecretsAsync(CancellationToken cancellationToken);

    }

}
