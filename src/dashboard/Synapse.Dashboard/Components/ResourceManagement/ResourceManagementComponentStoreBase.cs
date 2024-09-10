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
using Synapse.Dashboard.Components.DocumentDetailsStateManagement;

namespace Synapse.Dashboard.Components.ResourceManagement;

/// <summary>
/// Represents a <see cref="ComponentStore{TState}"/> used to manage Synapse <see cref="IResource"/>s of the specified type
/// </summary>
/// <typeparam name="TState">The type of the state managed by the component store</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to manage</typeparam>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public abstract class ResourceManagementComponentStoreBase<TState, TResource>(ILogger<ResourceManagementComponentStoreBase<TState, TResource>> logger, ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
     : ComponentStore<TState>(new())
    where TResource : Resource, new()
    where TState : ResourceManagementComponentState<TResource>, new()
{

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger<ResourceManagementComponentStoreBase<TState, TResource>> Logger { get; } = logger;

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="ResourceDefinition"/>s of the specified type
    /// </summary>
    public IObservable<ResourceDefinition?> Definition => this.Select(s => s.Definition).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe unfiltered <see cref="IResource"/>s of the specified type
    /// </summary>
    protected IObservable<EquatableList<TResource>?> InternalResources => this.Select(s => s.Resources).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="ResourceManagementComponentState{TResource}.SelectedResourceNames"/> changes
    /// </summary>
    public IObservable<EquatableList<string>> SelectedResourceNames => this.Select(s => s.SelectedResourceNames).DistinctUntilChanged();

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
        .Select(labelSelectors => new ResourcesFilter() { LabelSelectors = labelSelectors })
        .DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="ResourceManagementComponentState{TResource}.ActiveResourceName"/> changes
    /// </summary>
    public virtual IObservable<string> ActiveResourceName => this.Select(state => state.ActiveResourceName).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the active resource changes
    /// </summary>
    public virtual IObservable<TResource?> ActiveResource => Observable.CombineLatest(
        this.Resources.Where(resources => resources != null),
        this.ActiveResourceName.Where(name => !string.IsNullOrWhiteSpace(name)),
        (resources, name) => resources!.FirstOrDefault(r => r.GetName() == name)
    )
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
        this.ResourceWatch
            .Do(e => this.Logger.LogTrace("ResourceWatch received event '{type}' for '{name}'", e.Type.ToString(), e.Resource.GetName()))
            .SubscribeAsync(
                onNextAsync: this.OnResourceWatchEventAsync,
                onErrorAsync: ex => Task.Run(() => this.Logger.LogError("ResourceWatch exception: {exception}", ex.ToString())),
                onCompletedAsync: () => Task.CompletedTask,
                cancellationToken: this.CancellationTokenSource.Token
            );
        this.Filter.Throttle(TimeSpan.FromMilliseconds(10)).SubscribeAsync(
            onNextAsync: this.ListResourcesAsync,
            onErrorAsync: ex => Task.Run(() => this.Logger.LogError("Resource filter exception: {exception}", ex.ToString())),
            onCompletedAsync: () => Task.CompletedTask,
            cancellationToken: this.CancellationTokenSource.Token
        );
        await base.InitializeAsync();
    }

    /// <summary>
    /// Sets the <see cref="ResourceManagementComponentState{TResource}.SearchTerm" />
    /// </summary>
    /// <param name="searchTerm">The new search term</param>
    public virtual void SetSearchTerm(string? searchTerm)
    {
        this.Reduce(state => state with
        {
            SearchTerm = searchTerm
        });
    }

    /// <summary>
    /// Sets the <see cref="ResourceManagementComponentState{TResource}.ActiveResourceName" />
    /// </summary>
    /// <param name="activeResourceName">The new value</param>
    public virtual void SetActiveResourceName(string activeResourceName)
    {
        this.Reduce(state => state with
        {
            ActiveResourceName = activeResourceName ?? string.Empty
        });
    }

    /// <summary>
    /// Sets the <see cref="ResourceManagementComponentState{TResource}.LabelSelectors" />
    /// </summary>
    /// <param name="labelSelectors">The new label selectors</param>
    public virtual void SetLabelSelectors(EquatableList<LabelSelector>? labelSelectors)
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
    public virtual void AddLabelSelector(LabelSelector labelSelector)
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
    /// Toggles the resources selection
    /// </summary>
    /// <param name="name">The name of the resource to select, or all if none is provided</param>
    public virtual void ToggleResourceSelection(string? name = null)
    {
        this.Reduce(state =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (state.SelectedResourceNames.Count > 0)
                {
                    return state with
                    {
                        SelectedResourceNames = []
                    };
                }
                return state with
                {
                    SelectedResourceNames = [.. state.Resources?.Select(resource => resource.GetName()) ?? []]
                };
            }
            if (state.SelectedResourceNames.Contains(name))
            {
                return state with
                {
                    SelectedResourceNames = [.. state.SelectedResourceNames.Where(n => n != name)]
                };
            }
            return state with
            {
                SelectedResourceNames = [.. state.SelectedResourceNames, name]
            };
        });
    }

    /// <summary>
    /// Deletes the specified <see cref="IResource"/>
    /// </summary>
    /// <param name="resource">The <see cref="IResource"/> to delete</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public abstract Task DeleteResourceAsync(TResource resource);

    /// <summary>
    /// Deletes the selected <see cref="IResource"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task DeleteSelectedResourcesAsync()
    {
        var selectedResourcesNames = this.Get(state => state.SelectedResourceNames);
        var resources = (this.Get(state => state.Resources) ?? []).Where(resource => selectedResourcesNames.Contains(resource.GetName()));
        foreach (var resource in resources)
        {
            await this.DeleteResourceAsync(resource);
        }
        this.Reduce(state => state with
        {
            SelectedResourceNames = []
        });
    }

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
            var resourceList = new EquatableList<TResource>(await (await this.ApiClient.ManageNamespaced<TResource>().ListAsync(filter?.Namespace, filter?.LabelSelectors).ConfigureAwait(false)).OrderBy(r => r.Metadata.CreationTimestamp).ToListAsync().ConfigureAwait(false));
            this.Reduce(s => s with
            {
                Resources = resourceList,
                Loading = false
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to list resources: {exception}", ex.ToString());
        }
    }

    /// <summary>
    /// Handles the specified <see cref="IResourceWatchEvent"/>
    /// </summary>
    /// <param name="e">The <see cref="IResourceWatchEvent"/> to handle</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnResourceWatchEventAsync(IResourceWatchEvent<TResource> e)
    {
        var labelSelectors = this.Get(state => state.LabelSelectors);
        if (labelSelectors != null && labelSelectors.Count > 0 && !labelSelectors.All(selector => {
            if (e.Resource?.Metadata?.Labels == null || e.Resource.Metadata.Labels.Count == 0 || !e.Resource.Metadata.Labels.TryGetValue(selector.Key, out string? value))
            {
                return false;
            }
            var label = value;
            return selector.Operator switch
            {
                LabelSelectionOperator.Equals => !string.IsNullOrWhiteSpace(selector.Value) && selector.Value.Equals(label),
                LabelSelectionOperator.NotEquals => !string.IsNullOrWhiteSpace(selector.Value) && !selector.Value.Equals(label),
                LabelSelectionOperator.Contains => selector.Values != null && selector.Values.Contains(label),
                LabelSelectionOperator.NotContains => selector.Values != null && !selector.Values.Contains(label),
                _ => false,
            };
        }))
        {
            return Task.CompletedTask;
        }
        switch (e.Type)
        {
            case ResourceWatchEventType.Created:
                this.Reduce(state =>
                {
                    var resources = state.Resources == null ? [] : new EquatableList<TResource>(state.Resources);
                    if (resources.Any(r => r.GetQualifiedName() == e.Resource.GetQualifiedName()))
                    {
                        return state;
                    }
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
                    if (state.Resources == null) return state;
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
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public abstract class ResourceManagementComponentStoreBase<TResource>(ILogger<ResourceManagementComponentStoreBase<TResource>> logger, ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
     : ResourceManagementComponentStoreBase<ResourceManagementComponentState<TResource>, TResource>(logger, apiClient, resourceEventHub)
    where TResource : Resource, new()
{



}