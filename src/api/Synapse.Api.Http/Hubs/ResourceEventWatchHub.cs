using Synapse.Core.Api.Services;

namespace Synapse.Api.Http.Hubs;

/// <summary>
/// Represents the <see cref="Hub"/> used to notify clients about resource-related changes
/// </summary>
/// <remarks>
/// Initializes a new <see cref="ResourceEventWatchHub"/>
/// </remarks>
/// <param name="controller">The service used to control <see cref="ResourceEventWatchHub"/>s</param>
[Route("api/resource-management/v1/ws/watch")]
public class ResourceEventWatchHub(ResourceWatchEventHubController controller)
        : Hub<IResourceEventWatchHubClient>, IResourceEventWatchHub
{

    /// <summary>
    /// Gets the service used to control <see cref="ResourceEventWatchHub"/>s
    /// </summary>
    protected ResourceWatchEventHubController Controller { get; } = controller;

    /// <inheritdoc/>
    public virtual Task Watch(ResourceDefinitionInfo definition, string? @namespace = null) => this.Controller.WatchResourcesAsync(this.Context.ConnectionId, definition, @namespace);

    /// <inheritdoc/>
    public virtual Task StopWatching(ResourceDefinitionInfo definition, string? @namespace = null) => this.Controller.StopWatchingResourcesAsync(this.Context.ConnectionId, definition, @namespace);

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(Exception? exception) => this.Controller.ReleaseConnectionResourcesAsync(this.Context.ConnectionId);


}

