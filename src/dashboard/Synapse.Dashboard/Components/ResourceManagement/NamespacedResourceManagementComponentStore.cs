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
/// <typeparam name="TState">The type of the component's state</typeparam>
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to manage</typeparam>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public class NamespacedResourceManagementComponentStore<TState, TResource>(ILogger<NamespacedResourceManagementComponentStore<TState, TResource>> logger, ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
    : ResourceManagementComponentStoreBase<TState, TResource>(logger, apiClient, resourceEventHub)
    where TState : NamespacedResourceManagementComponentState<TResource>, new()
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="NamespacedResourceManagementComponentState{T}.ListNamespaces"/>
    /// </summary>
    public IObservable<bool> ListNamespaces => this.Select(s => s.ListNamespaces).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="NamespacedResourceManagementComponentState{T}.Namespace"/>s
    /// </summary>
    public IObservable<EquatableList<Namespace>?> Namespaces => this.Select(s => s.Namespaces).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe current namespace
    /// </summary>
    public IObservable<string?> Namespace => this.Select(s => s.Namespace).DistinctUntilChanged();

    /// <inheritdoc/>
    protected override IObservable<ResourcesFilter> Filter => Observable.CombineLatest(
            this.Namespace,
            this.LabelSelectors,
            (@namespace, labelSelectors) => new ResourcesFilter()
            {
                Namespace = @namespace,
                LabelSelectors = labelSelectors
            }
        )
        .DistinctUntilChanged();

    /// <summary>
    /// Sets the <see cref="NamespacedResourceManagementComponentState{TResource}.ListNamespaces"/> to true
    /// </summary>
    public void EnableNamespaceListing()
    {
        this.Reduce(state => state with
        {
            ListNamespaces = true
        });
    }
    /// <summary>
    /// Sets the <see cref="NamespacedResourceManagementComponentState{TResource}.ListNamespaces"/> to false
    /// </summary>
    public void DisableNamespaceListing()
    {
        this.Reduce(state => state with
        {
            ListNamespaces = false
        });
    }

    /// <summary>
    /// Sets the <see cref="NamespacedResourceManagementComponentState{TResource}.Namespace"/>
    /// </summary>
    /// <param name="namespace">The new namespace</param>
    public void SetNamespace(string? @namespace)
    {
        this.Reduce(state => state with
        {
            Namespace = @namespace
        });
    }

    /// <summary>
    /// Lists all available <see cref="Namespace"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task ListNamespacesAsync()
    {
        var namespaceList = new EquatableList<Namespace>(await (await this.ApiClient.Namespaces.GetAllAsync().ConfigureAwait(false)).OrderBy(ns => ns.GetQualifiedName()).ToListAsync().ConfigureAwait(false));
        this.Reduce(s => s with
        {
            Namespaces = namespaceList
        });
    }

    /// <inheritdoc/>
    public override async Task DeleteResourceAsync(TResource resource)
    {
        await this.ApiClient.ManageNamespaced<TResource>().DeleteAsync(resource.GetName(), resource.GetNamespace()!).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (this.Get(state => state.ListNamespaces)) await this.ListNamespacesAsync().ConfigureAwait(false);
    }

}
