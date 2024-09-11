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
using Synapse.Resources;

namespace Synapse.Dashboard.Pages.Workflows.List;

/// <summary>
/// Represents the <see cref="View"/>'s store
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="apiClient">The service used to interact with the Synapse API</param>
/// <param name="resourceEventHub">The hub used to watch resource events</param>
public class WorkflowListComponentStore(ILogger<WorkflowListComponentStore> logger, ISynapseApiClient apiClient, ResourceWatchEventHubClient resourceEventHub)
    : NamespacedResourceManagementComponentStore<WorkflowListState, Workflow>(logger, apiClient, resourceEventHub)
{

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowListState.Operators"/> changes
    /// </summary>
    public IObservable<EquatableList<Operator>?> Operators => this.Select(s => s.Operators).DistinctUntilChanged();

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe <see cref="WorkflowListState.Operator"/> changes
    /// </summary>
    public IObservable<string?> Operator => this.Select(s => s.Operator).DistinctUntilChanged();

    /// <summary>
    /// Lists all available <see cref="Operator"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public async Task ListOperatorsAsync()
    {
        var operatorList = new EquatableList<Operator>(await (await this.ApiClient.Operators.ListAsync().ConfigureAwait(false)).OrderBy(ns => ns.GetQualifiedName()).ToListAsync().ConfigureAwait(false));
        this.Reduce(s => s with
        {
            Operators = operatorList
        });
    }

    /// <summary>
    /// Sets the <see cref="WorkflowListState.Operator"/> 
    /// </summary>
    /// <param name="operatorName">The new value</param>
    public void SetOperator(string? operatorName)
    {
        this.Reduce(state => state with
        {
            Operator = operatorName
        });
    }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await this.ListOperatorsAsync().ConfigureAwait(false);
        this.Operator.Subscribe(operatorName => {
            if (string.IsNullOrWhiteSpace(operatorName))
            {
                this.RemoveLabelSelector(SynapseDefaults.Resources.Labels.Operator);
            }
            else
            {
                this.AddLabelSelector(new(SynapseDefaults.Resources.Labels.Operator, LabelSelectionOperator.Equals, operatorName));
            }
        }, token: this.CancellationTokenSource.Token);
    }
}
