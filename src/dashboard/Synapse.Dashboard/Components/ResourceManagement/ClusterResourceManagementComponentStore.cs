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
/// <typeparam name="TResource">The type of <see cref="IResource"/>s to manage</typeparam>
/// <remarks>
/// Initializes a new <see cref="ClusterResourceManagementComponentStore{TResource}"/>
/// </remarks>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The <see cref="IResourceEventWatchHub"/> websocket service client</param>
public class ClusterResourceManagementComponentStore<TResource>(ILogger<ClusterResourceManagementComponentStore<TResource>> logger, ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
    : ResourceManagementComponentStoreBase<TResource>(logger, apiClient, resourceEventHub)
    where TResource : Resource, new()
{

    /// <inheritdoc/>
    public override async Task DeleteResourceAsync(TResource resource)
    {
        await this.ApiClient.ManageCluster<TResource>().DeleteAsync(resource.GetName()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task GetResourceDefinitionAsync()
    {
        var resourceDefinition = await this.ApiClient.ManageCluster<TResource>().GetDefinitionAsync().ConfigureAwait(false);
        this.Reduce(s => s with
        {
            Definition = resourceDefinition
        });
    }

    /// <inheritdoc/>
    public override async Task ListResourcesAsync(ResourcesFilter? filter = null)
    {
        try
        {
            this.Reduce(state => state with
            {
                Loading = true,
            });
            var resourceList = new EquatableList<TResource>(await (await this.ApiClient.ManageCluster<TResource>().ListAsync(filter?.LabelSelectors).ConfigureAwait(false)).OrderBy(r => r.Metadata.CreationTimestamp).ToListAsync().ConfigureAwait(false));
            this.Reduce(s => s with
            {
                Resources = resourceList,
                Loading = false
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Unable to list resources, {exception}", ex.ToString());
        }
    }


}
