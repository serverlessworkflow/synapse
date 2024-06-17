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

using Synapse.Resources;

namespace Synapse.Dashboard.Pages.WorkflowInstances.List;

/// <summary>
/// Represents the component use to list workflow instances
/// </summary>
public class WorkflowInstanceListComponent
    : NamespacedResourceManagementComponent<WorkflowInstanceListComponentStore, WorkflowInstanceListState, WorkflowInstance>
{

    /// <summary>
    /// Gets the list of available <see cref="Workflow"/>s
    /// </summary>
    protected EquatableList<Workflow>? Workflows { get; set; }

    /// <summary>
    /// Gets the name of the current <see cref="Namespace"/>s
    /// </summary>
    protected string? Namespace { get; set; }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        this.Store.Workflows.Subscribe(OnWorkflowCollectionChanged, token: this.CancellationTokenSource.Token);
        this.Store.Namespace.Subscribe(OnNamespaceChanged, token: this.CancellationTokenSource.Token);
        await this.Store.ListWorkflowsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the component
    /// </summary>
    /// <param name="workflows">The current <see cref="Workflow"/>s</param>
    protected void OnWorkflowCollectionChanged(EquatableList<Workflow>? workflows)
    {
        this.Workflows = workflows;
        this.StateHasChanged();
    }

    /// <summary>
    /// Updates the component
    /// </summary>
    /// <param name="namespace">The current <see cref="Namespace"/>s</param>
    protected void OnNamespaceChanged(string? @namespace)
    {
        this.Namespace = @namespace;
        this.StateHasChanged();
    }

}
