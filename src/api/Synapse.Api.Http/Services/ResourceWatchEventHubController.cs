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

namespace Synapse.Core.Api.Services;

/// <summary>
/// Represents a service used to dispatch <see cref="ResourceWatchEvent"/>s to all <see cref="IResourceEventWatchHubClient"/>s
/// </summary>
/// <remarks>
/// Initializes a new <see cref="ResourceWatchEventHubController"/>
/// </remarks>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="hubContext">The current <see cref="IResourceEventWatchHubClient"/>'s <see cref="IHubContext{THub, T}"/></param>
public class ResourceWatchEventHubController(IServiceProvider serviceProvider, IHubContext<ResourceEventWatchHub, IResourceEventWatchHubClient> hubContext)
    : BackgroundService
{

    /// <summary>
    /// Gets the current <see cref="IServiceScope"/>
    /// </summary>
    protected IServiceScope ServiceScope { get; } = serviceProvider.CreateScope();

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider => this.ServiceScope.ServiceProvider;

    /// <summary>
    /// Gets the service used to manage <see cref="IResource"/>s
    /// </summary>
    protected IResourceRepository Resources => this.ServiceProvider.GetRequiredService<IResourceRepository>();

    /// <summary>
    /// Gets the current <see cref="IResourceEventWatchHubClient"/>'s <see cref="IHubContext{THub, T}"/>
    /// </summary>
    protected IHubContext<ResourceEventWatchHub, IResourceEventWatchHubClient> HubContext { get; } = hubContext;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the mapping of active ^subscriptions per hub connection id
    /// </summary>
    protected ConcurrentDictionary<string, ConcurrentDictionary<string, IResourceWatch>> Connections { get; } = new();

    /// <summary>
    /// Gets the <see cref="ResourceWatchEventHubController"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; private set; } = null!;

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Watches resources of the specified type
    /// </summary>
    /// <param name="connectionId">The id of the SignalR connection to create the watch for</param>
    /// <param name="definition">The type of resource to watch</param>
    /// <param name="namespace">The namespace resources to watch belong to, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task WatchResourcesAsync(string connectionId, ResourceDefinitionInfo definition, string? @namespace = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionId)) throw new ArgumentNullException(nameof(connectionId));
        ArgumentNullException.ThrowIfNull(definition);
        var subscriptionKey = this.GetSubscriptionKey(definition, @namespace);
        if (this.Connections.TryGetValue(connectionId, out var subscriptions) && subscriptions != null && subscriptions.TryGetValue(subscriptionKey, out var watch) && watch != null) return;
        if (subscriptions == null)
        {
            subscriptions = new();
            this.Connections.AddOrUpdate(connectionId, subscriptions, (key, current) =>
            {
                current.Values.ToList().ForEach(d => d.Dispose());
                return subscriptions;
            });
        }
        watch = await this.Resources.WatchAsync(definition.Group, definition.Version, definition.Plural, @namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
        watch.SubscribeAsync(e => this.OnResourceWatchEventAsync(connectionId, e));
        subscriptions.AddOrUpdate(subscriptionKey, watch, (key, current) =>
        {
            current.Dispose();
            return watch;
        });
    }

    /// <summary>
    /// Stop watching resources of the specified type
    /// </summary>
    /// <param name="connectionId">The id of the SignalR connection that owns the watch to dispose of</param>
    /// <param name="definition">The type of resource to stop watching</param>
    /// <param name="namespace">The namespace resources to stop watching belong to, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual Task StopWatchingResourcesAsync(string connectionId, ResourceDefinitionInfo definition, string? @namespace = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionId)) throw new ArgumentNullException(nameof(connectionId));
        ArgumentNullException.ThrowIfNull(definition);
        if (!this.Connections.TryGetValue(connectionId, out var subscriptions) || subscriptions == null || subscriptions.IsEmpty) return Task.CompletedTask;
        var subscriptionKey = this.GetSubscriptionKey(definition, @namespace);
        if (subscriptions.Remove(subscriptionKey, out var subscription) && subscription != null) subscription.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Releases all resources owned by the specified connection
    /// </summary>
    /// <param name="connectionId">The id of the connection to release the resources of</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual Task ReleaseConnectionResourcesAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionId)) throw new ArgumentNullException(nameof(connectionId));
        if (!this.Connections.Remove(connectionId, out var subscriptions) || subscriptions == null || subscriptions.IsEmpty) return Task.CompletedTask;
        subscriptions.Keys.ToList().ForEach(subscriptionId =>
        {
            subscriptions.Remove(subscriptionId, out var subscription);
            subscription?.Dispose();
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new subscription key for the specified resource type and namespace
    /// </summary>
    /// <param name="definition">The type of resources to create a new subscription key for</param>
    /// <param name="namespace">The namespace the resources to create a new subscription key for belong to</param>
    /// <returns>A new subscription key for the specified resource type and namespace</returns>
    protected virtual string GetSubscriptionKey(ResourceDefinitionInfo definition, string? @namespace = null) => string.IsNullOrWhiteSpace(@namespace) ? definition.ToString() : $"{definition}/{@namespace}";

    /// <summary>
    /// Handles the specified <see cref="IResourceWatchEvent"/>
    /// </summary>
    /// <param name="connectionId">The id of the connection the event has been produced for</param>
    /// <param name="e">The <see cref="IResourceWatchEvent"/> to handle</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceWatchEventAsync(string connectionId, IResourceWatchEvent e) => this.HubContext.Clients.Client(connectionId).ResourceWatchEvent(e);

    /// <inheritdoc/>
    public override void Dispose()
    {
        this.CancellationTokenSource?.Dispose();
        this.Connections.Keys.ToList().ForEach(connectionId =>
        {
            this.Connections.Remove(connectionId, out var subscriptions);
            if (subscriptions == null) return;
            subscriptions.Keys.ToList().ForEach(subscriptionId =>
            {
                subscriptions.Remove(subscriptionId, out var subscription);
                subscription?.Dispose();
            });
        });
        this.ServiceScope.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

}