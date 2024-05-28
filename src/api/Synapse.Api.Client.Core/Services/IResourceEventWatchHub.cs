namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used by clients to watch resource-related events
/// </summary>
public interface IResourceEventWatchHub
{

    /// <summary>
    /// Subscribes to events produced by resources of the specified type
    /// </summary>
    /// <param name="resourceDefinition">The type of resources to watch</param>
    /// <param name="namespace">The namespace the resources to watch belong to, if any</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task Watch(ResourceDefinitionInfo resourceDefinition, string? @namespace = null);

    /// <summary>
    /// Unsubscribes from events produced by resources of the specified type
    /// </summary>
    /// <param name="resourceDefinition">The type of resources to stop watching</param>
    /// <param name="namespace">The namespace the resources to stop watching belong to, if any</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task StopWatching(ResourceDefinitionInfo resourceDefinition, string? @namespace = null);

}
