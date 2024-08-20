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

namespace Synapse.Dashboard.Components.WorkflowDiagramStateManagement;

/// <summary>
/// Represents the state of a <see cref="WorkflowDiagram"/>
/// </summary>
public record WorkflowDiagramState
{
    /// <summary>
    /// Gets/sets the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/> to build a diagram for
    /// </summary>
    public WorkflowDefinition? WorkflowDefinition { get; set; } = null;

    /// <summary>
    /// Gets/sets the <see cref="WorkflowDiagramOrientation"/> of the diagram
    /// </summary>
    public WorkflowDiagramOrientation Orientation { get; set; } = default!;

    /// <summary>
    /// Gets/sets the <see cref="WorkflowInstance"/>s to get the activity counts from
    /// </summary>
    public EquatableList<WorkflowInstance> WorkflowInstances { get; set; } = [];
}
