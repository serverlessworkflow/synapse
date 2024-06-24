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

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceDefinition"/>s of the specified type
    /// </summary>
    public IObservable<ResourceDefinition?> Definition => this.Select(s => s.Definition).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe unfiltered <see cref="IResource"/>s of the specified type
    /// </summary>
    protected IObservable<EquatableList<TResource>?> InternalResources => this.Select(s => s.Resources).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the  <see cref="ResourceManagementComponentState{TResource}.SearchTerm"/> changes
    /// </summary>
    public IObservable<string?> SearchTerm => this.Select(state => state.SearchTerm).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="ResourceManagementComponentState{TResource}.LabelSelectors"/> changes
    /// </summary>
    public IObservable<EquatableList<LabelSelector>?> LabelSelectors => this.Select(state => state.LabelSelectors).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe changes
    /// </summary>
    public IObservable<bool> Loading => this.Select(state => state.Loading).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="IResource"/>s of the specified type
    /// </summary>
    public IObservable<EquatableList<TResource>?> Resources => Observable.CombineLatest(
            this.InternalResources,
            this.SearchTerm.Throttle(TimeSpan.FromMilliseconds(100)).StartWith(""),
            (resources, searchTerm) =>
            {
                if (resources == null)
                {
                    return [];
                }
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return resources!;
                }
                return new EquatableList<TResource>(resources!.Where(r => r.GetName().Contains(searchTerm)));
            }
         )
        .DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="ResourcesFilter"/> 
    /// </summary>
    protected virtual IObservable<ResourcesFilter> Filter => this.LabelSelectors
        .Select(labelSelectors =>  new ResourcesFilter() { LabelSelectors = labelSelectors })
        .DistinctUntilChanged();

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


    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await this.GetResourceDefinitionAsync().ConfigureAwait(false);
        await this.ResourceEventHub.StartAsync().ConfigureAwait(false);
        this.ResourceWatch = await this.ResourceEventHub.WatchAsync<TResource>().ConfigureAwait(false);
        this.ResourceWatch.SubscribeAsync(
            onNextAsync: this.OnResourceWatchEventAsync, 
            onErrorAsync: ex => Task.Run(() => Console.WriteLine(ex)), 
            onCompletedAsync: () => Task.CompletedTask, 
            cancellationToken: this.CancellationTokenSource.Token
        );
        this.Filter.SubscribeAsync(
            onNextAsync: this.ListResourcesAsync,
            onErrorAsync: ex => Task.Run(() => Console.WriteLine(ex)),
            onCompletedAsync: () => Task.CompletedTask,
            cancellationToken: this.CancellationTokenSource.Token
        );
        await base.InitializeAsync();
    }

    /// <summary>
    /// Sets the <see cref="ResourceManagementComponentState{TResource}.SearchTerm" />
    /// </summary>
    /// <param name="searchTerm">The new search term</param>
    public void SetSearchTerm(string? searchTerm)
    {
        this.Reduce(state => state with
        {
            SearchTerm = searchTerm
        });
    }

    /// <summary>
    /// Sets the <see cref="ResourceManagementComponentState{TResource}.LabelSelectors" />
    /// </summary>
    /// <param name="labelSelectors">The new label selectors</param>
    public void SetLabelSelectors(EquatableList<LabelSelector>? labelSelectors)
    {
        this.Reduce(state => state with
        {
            LabelSelectors = new EquatableList<LabelSelector>(labelSelectors ?? [])
        });
    }

    /// <summary>
    /// Adds a single <see cref="LabelSelector" />
    /// </summary>
    /// <param name="labelSelector">The label selector to add</param>
    public void AddLabelSelector(LabelSelector labelSelector)
    {
        if (labelSelector == null)
        {
            return;
        }
        var labelSelectors = new EquatableList<LabelSelector>(this.Get(state => state.LabelSelectors) ?? []);
        var existingSelector = labelSelectors.FirstOrDefault(selector => selector.Key == labelSelector.Key);
        if (existingSelector != null)
        {
            labelSelectors.Remove(existingSelector);
        }
        labelSelectors.Add(labelSelector);
        this.SetLabelSelectors(labelSelectors);
    }

    /// <summary>
    /// Removes a single <see cref="LabelSelector" /> by key
    /// </summary>
    /// <param name="labelSelectorKey">The label selector key to remove</param>
    public void RemoveLabelSelector(string labelSelectorKey)
    {
        if (string.IsNullOrWhiteSpace(labelSelectorKey))
        {
            return;
        }
        var labelSelectors = new EquatableList<LabelSelector>(this.Get(state => state.LabelSelectors) ?? []);
        var existingSelector = labelSelectors.FirstOrDefault(selector => selector.Key == labelSelectorKey);
        if (existingSelector != null)
        {
            labelSelectors.Remove(existingSelector);
        }
        this.SetLabelSelectors(labelSelectors);
    }

    /// <summary>
    /// Deletes the specified <see cref="IResource"/>
    /// </summary>
    /// <param name="resource">The <see cref="IResource"/> to delete</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public abstract Task DeleteResourceAsync(TResource resource);

    /// <summary>
    /// Fetches the definition of the managed <see cref="IResource"/> type
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task GetResourceDefinitionAsync()
    {
        var resourceDefinition = await this.ApiClient.ManageNamespaced<TResource>().GetDefinitionAsync().ConfigureAwait(false);
        this.Reduce(s => s with
        {
            Definition = resourceDefinition
        });
    }

    /// <summary>
    /// Lists all the resources managed by Synapse
    /// </summary>
    /// <param name="filter">The <see cref="ResourcesFilter" />, if any, to list the resources of</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task ListResourcesAsync(ResourcesFilter? filter = null)
    {
        try
        {
            this.Reduce(state => state with
            {
                Loading = true,
            });
            var resourceList = new EquatableList<TResource>(await (await this.ApiClient.ManageNamespaced<TResource>().ListAsync(filter?.Namespace, filter?.LabelSelectors).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false));
            this.Reduce(s => s with
            {
                Resources = resourceList,
                Loading = false
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            // todo: implement proper error handling
        }
    }

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