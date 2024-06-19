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
using Synapse.Resources;
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.WorkflowInstances.List;

/// <summary>
/// Represents the <see cref="Synapse.Dashboard.Pages.WorkflowInstances.List.View"/>'s store
/// </summary>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The hub used to watch resource events</param>
public class WorkflowInstanceListComponentStore(ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
    : NamespacedResourceManagementComponentStore<WorkflowInstanceListState, WorkflowInstance>(apiClient, resourceEventHub)
{

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="Workflow"/>s
    /// </summary>
    public IObservable<EquatableList<Workflow>?> Workflows => this.Select(s => s.Workflows);


    /// <summary>
    /// Lists all available <see cref="Workflow"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task ListWorkflowsAsync()
    {
        this.Reduce(state => state with
        {
            Loading = true
        });
        var workflowList = new EquatableList<Workflow>(await (await this.ApiClient.Workflows.ListAsync(null!).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false));
        this.Reduce(s => s with
        {
            Workflows = workflowList,
            Loading = false
        });
    }

}
