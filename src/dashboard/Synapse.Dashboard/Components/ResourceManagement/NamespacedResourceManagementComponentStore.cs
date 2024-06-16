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
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to manage</typeparam>
/// <remarks>
/// Initializes a new <see cref="NamespacedResourceManagementComponentStore{TResource}"/>
/// </remarks>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public class NamespacedResourceManagementComponentStore<TResource>(ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
    : ResourceManagementComponentStoreBase<TResource>(apiClient, resourceEventHub)
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="Namespace"/>s
    /// </summary>
    public IObservable<EquatableList<Namespace>?> Namespaces => this.Select(s => s.Namespaces);

    /// <summary>
    /// Gets a list containing local copies of managed <see cref="Namespace"/>s
    /// </summary>
    protected EquatableList<Namespace>? NamespaceList { get; set; }

    /// <inheritdoc/>
    public override async Task GetResourceDefinitionAsync()
    {
        this.ResourceDefinition = await this.ApiClient.ManageNamespaced<TResource>().GetDefinitionAsync().ConfigureAwait(false);
        this.Reduce(s => s with
        {
            Definition = this.ResourceDefinition
        });
    }

    /// <summary>
    /// Lists all resources within the specified namespace
    /// </summary>
    /// <param name="namespace">The namespace, if any, to list the resources of</param>
    /// <param name="labelSelectors">A list of the label selectors, if any, used to filter the resources to list</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task ListResourcesAsync(string? @namespace, IEnumerable<LabelSelector>? labelSelectors)
    {
        this.Reduce(state => state with
        {
            Loading = true,
        });
        this.ResourceList = new EquatableList<TResource>(await (await this.ApiClient.ManageNamespaced<TResource>().ListAsync(@namespace, labelSelectors).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false));
        this.Reduce(s => s with
        {
            Resources = this.ResourceList,
            Namespace = @namespace,
            LabelSelectors = labelSelectors == null ? null : new(labelSelectors),
            Loading = false
        });
    }

    /// <inheritdoc/>
    public override Task ListResourcesAsync(IEnumerable<LabelSelector>? labelSelectors) => this.ListResourcesAsync(null, labelSelectors);

    /// <summary>
    /// Lists all available <see cref="Namespace"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task ListNamespaceAsync()
    {
        this.Reduce(state => state with
        {
            Loading = true
        });
        this.NamespaceList = new EquatableList<Namespace>(await (await this.ApiClient.Namespaces.ListAsync().ConfigureAwait(false)).ToListAsync().ConfigureAwait(false));
        this.Reduce(s => s with
        {
            Namespaces = this.NamespaceList,
            Loading = false
        });
    }

    /// <inheritdoc/>
    public override async Task DeleteResourceAsync(TResource resource)
    {
        await this.ApiClient.ManageNamespaced<TResource>().DeleteAsync(resource.GetName(), resource.GetNamespace()!).ConfigureAwait(false);
        var match = this.ResourceList?.ToList().FirstOrDefault(r => r.GetName() == resource.GetName() && r.GetNamespace() == resource.GetNamespace());
        var resourceCollectionChanged = false;
        if (match != null)
        {
            this.ResourceList!.Remove(match);
            resourceCollectionChanged = true;
        }
        if (!resourceCollectionChanged) return;
        this.Reduce(s => s with
        {
            Resources = this.ResourceList
        });
    }

}
