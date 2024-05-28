namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used by clients to watch resource-related events
/// </summary>
public interface IResourceEventWatchHubClient
{

    /// <summary>
    /// Notifies clients about a resource-related event
    /// </summary>
    /// <param name="e">The <see cref="ResourceWatchEvent"/> that has been produced</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ResourceWatchEvent(object e);

}
