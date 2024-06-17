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

using Synapse.Api.Client.Services;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Components.ResourceManagement;

/// <summary>
/// Represents a <see cref="ComponentStore{TState}"/> used to manage Synapse <see cref="IResource"/>s of the specified type
/// </summary>
/// <typeparam name="TState">The type of the state managed by the component store</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to manage</typeparam>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public abstract class ResourceManagementComponentStoreBase<TState, TResource>(ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
     : ComponentStore<TState>(new())
    where TResource : Resource, new()
    where TState : ResourceManagementComponentState<TResource>, new()
{

    EquatableList<TResource>? _unfilteredResourceList;

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceDefinition"/>s of the specified type
    /// </summary>
    public IObservable<ResourceDefinition?> Definition => this.Select(s => s.Definition);

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="IResource"/>s of the specified type
    /// </summary>
    public IObservable<EquatableList<TResource>?> Resources => this.Select(s => s.Resources);

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes
    /// </summary>
    public IObservable<bool> Loading => this.Select(state => state.Loading).DistinctUntilChanged();

    /// <summary>
    /// Gets the service used to interact with the Synapse API
    /// </summary>
    protected ISynapseApiClient ApiClient { get; } = apiClient;

    /// <summary>
    /// Gets the <see cref="IResourceEventWatchHub"/> websocket service client
    /// </summary>
    protected ResourceWatchEventHubClient ResourceEventHub { get; } = resourceEventHub;

    /// <summary>
    /// Gets the service used to monitor resources of the specified type
    /// </summary>
    protected Api.Client.Services.ResourceWatch<TResource> ResourceWatch { get; private set; } = null!;

    /// <summary>
    /// Gets an <see cref="IDisposable"/> that represents the store's <see cref="ResourceWatch"/> subscription
    /// </summary>
    protected IDisposable ResourceWatchSubscription { get; private set; } = null!;

    /// <summary>
    /// Gets the definition of managed <see cref="IResource"/>s
    /// </summary>
    protected ResourceDefinition? ResourceDefinition { get; set; }

    /// <summary>
    /// Gets a list containing local copies of managed <see cref="IResource"/>s
    /// </summary>
    protected EquatableList<TResource>? ResourceList { get; set; }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await this.ResourceEventHub.StartAsync().ConfigureAwait(false);
        this.ResourceWatch = await this.ResourceEventHub.WatchAsync<TResource>().ConfigureAwait(false);
        this.ResourceWatch.SubscribeAsync(OnResourceWatchEventAsync, onErrorAsync: ex => Task.Run(() => Console.WriteLine(ex)));
        await base.InitializeAsync();
    }

    /// <summary>
    /// Fetches the definition of the managed <see cref="IResource"/> type
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public abstract Task GetResourceDefinitionAsync();

    /// <summary>
    /// Lists all the resources managed by Synapse
    /// </summary>
    /// <param name="labelSelectors">A list containing the label selectors, if any, used to filter the resources to list</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public abstract Task ListResourcesAsync(IEnumerable<LabelSelector>? labelSelectors = null);

    /// <summary>
    /// Searches resources for the specified term
    /// </summary>
    /// <param name="term">The term to search resources by</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual Task SearchResourcesAsync(string? term)
    {
        this.Reduce(state => state with
        {
            Loading = true
        });
        if (string.IsNullOrWhiteSpace(term))
        {
            if (_unfilteredResourceList != null) ResourceList = new(_unfilteredResourceList);
            _unfilteredResourceList = null;
        }
        else
        {
            if (_unfilteredResourceList != null) ResourceList = _unfilteredResourceList;
            else if(ResourceList != null) _unfilteredResourceList = new(ResourceList);
            ResourceList = ResourceList == null ? null : new(ResourceList.Where(r => r.GetName().StartsWith(term)));
        }
        this.Reduce(s => s with
        {
            Resources = this.ResourceList,
            Loading = false
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes the specified <see cref="IResource"/>
    /// </summary>
    /// <param name="resource">The <see cref="IResource"/> to delete</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public abstract Task DeleteResourceAsync(TResource resource);

    /// <summary>
    /// Handles the specified <see cref="IResourceWatchEvent"/>
    /// </summary>
    /// <param name="e">The <see cref="IResourceWatchEvent"/> to handle</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceWatchEventAsync(IResourceWatchEvent<TResource> e)
    {
        switch (e.Type)
        {
            case ResourceWatchEventType.Created:
                this.Reduce(state =>
                {
                    var resources = state.Resources == null ? [] : new EquatableList<TResource>(state.Resources);
                    resources.Add(e.Resource);
                    return state with
                    {
                        Resources = resources
                    };
                });
                break;
            case ResourceWatchEventType.Updated:
                this.Reduce(state =>
                {
                    if (state.Resources == null)
                    {
                        return state;
                    }
                    var resources = new EquatableList<TResource>(state.Resources);
                    var resource = resources.FirstOrDefault(r => r.GetQualifiedName() == e.Resource.GetQualifiedName());
                    if (resource == null) return state;
                    var index = resources.IndexOf(resource);
                    resources.Remove(resource);
                    resources.Insert(index, e.Resource);
                    return state with
                    {
                        Resources = resources
                    };
                });
                break;
            case ResourceWatchEventType.Deleted:
                this.Reduce(state =>
                {
                    if (state.Resources == null)
                    {
                        return state;
                    }
                    var resources = new EquatableList<TResource>(state.Resources);
                    var resource = resources.FirstOrDefault(r => r.GetQualifiedName() == e.Resource.GetQualifiedName());
                    if (resource == null) return state;
                    resources.Remove(resource);
                    return state with
                    {
                        Resources = resources
                    };
                });
                break;
            default:
                throw new NotSupportedException($"The specified {nameof(ResourceWatchEventType)} '{e.Type}' is not supported");
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        this.ResourceWatchSubscription?.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing) return;
        await this.ResourceWatch.DisposeAsync().ConfigureAwait(false);
        this.ResourceWatchSubscription.Dispose();
        base.Dispose(disposing);
    }

}

/// <summary>
/// Represents a <see cref="ComponentStore{TState}"/> used to manage Synapse <see cref="IResource"/>s of the specified type
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to manage</typeparam>
/// <remarks>
/// Initializes a new <see cref="ResourceManagementComponentStoreBase{TResource}"/>
/// </remarks>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public abstract class ResourceManagementComponentStoreBase<TResource>(ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
     : ResourceManagementComponentStoreBase<ResourceManagementComponentState<TResource>, TResource>(apiClient, resourceEventHub)
    where TResource : Resource, new()
{



}