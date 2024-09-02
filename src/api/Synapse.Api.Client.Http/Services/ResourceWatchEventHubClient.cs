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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the service used to listen to <see cref="CloudEvent"/>s in real-time
/// </summary>
public class ResourceWatchEventHubClient
    : IAsyncDisposable
{
    
    bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ResourceWatchEventHubClient"/>
    /// </summary>
    /// <param name="connection">The underlying <see cref="HubConnection"/></param>
    public ResourceWatchEventHubClient(HubConnection connection)
    {
        this.Connection = connection;
        this.Connection.On<ResourceWatchEvent>(nameof(IResourceEventWatchHubClient.ResourceWatchEvent), this.WatchEventStream.OnNext);
    }

    /// <summary>
    /// Gets the underlying <see cref="HubConnection"/>
    /// </summary>
    protected HubConnection Connection { get; }

    /// <summary>
    /// Gets the <see cref="Subject{T}"/> used to notify subscribers about incoming <see cref="IResourceWatchEvent"/>s
    /// </summary>
    protected Subject<ResourceWatchEvent> WatchEventStream { get; } = new();

    /// <summary>
    /// Starts the <see cref="ResourceWatchEventHubClient"/> if it's not already running
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual Task StartAsync() => this.Connection.State == HubConnectionState.Disconnected ? this.Connection.StartAsync() : Task.CompletedTask;

    /// <summary>
    /// Watches resource of the specified type
    /// </summary>
    /// <typeparam name="TResource">The type of resources to watch</typeparam>
    /// <param name="namespace">The namespace resources to watch belong to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe incoming <see cref="IResourceWatchEvent"/>s for resources of the specified type</returns>
    public virtual async Task<ResourceWatch<TResource>> WatchAsync<TResource>(string? @namespace = null, CancellationToken cancellationToken = default)
        where TResource : class, IResource, new()
    {
        var resource = new TResource();
        await this.Connection.InvokeAsync(nameof(IResourceEventWatchHub.Watch), resource.Definition, @namespace, cancellationToken).ConfigureAwait(false);
        var stream = this.WatchEventStream
            .Where(e => e.Resource.IsOfType<TResource>())
            .Select(e => e.OfType<TResource>());
        if(!string.IsNullOrWhiteSpace(@namespace)) stream = stream.Where(e => e.Resource.GetNamespace() == @namespace);
        return new(this, @namespace, stream);
    }

    /// <summary>
    /// Stops watching resources of the specified type
    /// </summary>
    /// <typeparam name="TResource">The type of resources to stop watching</typeparam>
    /// <param name="namespace">The namespace resources to stop watching belong to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task StopWatchingAsync<TResource>(string? @namespace = null, CancellationToken cancellationToken = default)
        where TResource : class, IResource, new()
    {
        var resource = new TResource();
        await this.Connection.InvokeAsync(nameof(IResourceEventWatchHub.StopWatching), resource.Definition, @namespace, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes of the <see cref="ResourceWatchEventHubClient"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="ResourceWatchEventHubClient"/> is being disposed of</param>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (this._disposed) return;
        if (disposing)
        {
            this.WatchEventStream.Dispose();
            await this.Connection.DisposeAsync();
        }
        this._disposed = true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

}
