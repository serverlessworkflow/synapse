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

namespace Synapse.Dashboard.Pages.Workflows.Create;

/// <summary>
/// The <see cref="State{TState}"/> of the workflow editor
/// </summary>
[Feature]
public record CreateWorkflowViewState
{

    /// <summary>
    /// Gets/sets the <see cref="StandaloneCodeEditor"/> used to code the workflow definition to create
    /// </summary>
    public StandaloneCodeEditor TextEditor { get; set; } = null!;

    /// <summary>
    /// Gets/sets the workflow that defines the created workflow definition
    /// </summary>
    public Workflow? Workflow { get; set; }

    /// <summary>
    /// Gets/sets the definition of the workflow to create
    /// </summary>
    public WorkflowDefinition? WorkflowDefinition { get; set; }

    /// <summary>
    /// Gets/sets the workflow definition text representation
    /// </summary>
    public string? WorkflowDefinitionText { get; set; }

    /// <summary>
    /// Gets/sets a boolean indicating whether or not the state is being loaded
    /// </summary>
    public bool Loading { get; set; }

    /// <summary>
    /// Defines if the workflow definition is being updated
    /// </summary>
    public bool Updating { get; set; }

    /// <summary>
    /// Defines if the workflow definition is being saved
    /// </summary>
    public bool Saving { get; set; }

}
