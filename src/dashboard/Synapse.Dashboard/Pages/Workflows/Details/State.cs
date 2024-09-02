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

using ServerlessWorkflow.Sdk.Models;
using Synapse.Resources;

namespace Synapse.Dashboard.Pages.Workflows.Details;

/// <summary>
/// Represents the <see cref="View"/>'s state
/// </summary>
public record WorkflowDetailsState
    : NamespacedResourceManagementComponentState<WorkflowInstance>
{
    /// <summary>
    /// Gets/sets the displayed <see cref="Workflow"/>
    /// </summary>
    public Workflow? Workflow { get; set; }

    /// <summary>
    /// Gets/sets the displayed <see cref="Workflow"/>'s <see cref="WorkflowDefinition"/> version
    /// </summary>
    public string? WorkflowDefinitionVersion { get; set; }

    /// <summary>
    /// Gets/sets a <see cref="List{T}"/> that contains all <see cref="Workflow"/>s variations
    /// </summary>
    public EquatableList<Workflow>? Workflows { get; set; }

    /// <summary>
    /// Gets/sets the parsed <see cref="WorkflowDefinition"/>
    /// </summary>
    public string WorkflowDefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the displayed <see cref="WorkflowInstance"/> id
    /// </summary>
    public string? WorkflowInstanceName { get; set; }

}
