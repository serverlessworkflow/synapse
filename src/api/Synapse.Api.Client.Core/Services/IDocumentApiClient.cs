namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of the Synapse API used to manage <see cref="Document"/>s
/// </summary>
public interface IDocumentApiClient
{

    /// <summary>
    /// Creates a new <see cref="Document"/>
    /// </summary>
    /// <param name="name">The <see cref="Document"/>'s name</param>
    /// <param name="content">The <see cref="Document"/>'s content</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The newly created <see cref="Document"/></returns>
    Task<Document> CreateAsync(string name, object? content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the <see cref="Document"/> with the specified id
    /// </summary>
    /// <param name="id">The id of the <see cref="Document"/> to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Document"/> with the specified id</returns>
    Task<Document> GetAsync(string id, CancellationToken cancellationToken = default);

}
