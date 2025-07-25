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
    /// Gets/sets the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/>'s <see cref="Namespace"/>
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/>'s name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="ServerlessWorkflow.Sdk.Models.WorkflowDefinition"/>'s  DSL version
    /// </summary>
    public string? DslVersion { get; set; }

    /// <summary>
    /// Gets/sets a boolean determining if the workflow is new or if it's an update
    /// </summary>
    public bool IsNew { get; set; } = true;

    /// <summary>
    /// Gets/sets the definition of the workflow to create
    /// </summary>
    public WorkflowDefinition WorkflowDefinition { get; set; } = new WorkflowDefinition()
    {
        Document = new()
        {
            Dsl = "1.0.0-alpha5",
            Namespace = Neuroglia.Data.Infrastructure.ResourceOriented.Namespace.DefaultNamespaceName,
            Name = "new-workflow",
            Version = "0.1.0"
        },
        Do = []
    };

    /// <summary>
    /// Gets/sets the workflow definition text representation
    /// </summary>
    public string? WorkflowDefinitionText { get; set; }

    /// <summary>
    /// Gets/sets the workflow's labels
    /// </summary>
    public EquatableDictionary<string, string> Labels { get; set; } = [];

    /// <summary>
    /// Gets/sets the workflow's annotations
    /// </summary>
    public EquatableDictionary<string, string> Annotations { get; set; } = [];

    /// <summary>
    /// Gets/sets a boolean indicating whether or not the state is being loaded
    /// </summary>
    public bool Loading { get; set; } = false;

    /// <summary>
    /// Defines if the workflow definition is being updated
    /// </summary>
    public bool Updating { get; set; } = false;

    /// <summary>
    /// Defines if the workflow definition is being saved
    /// </summary>
    public bool Saving { get; set; } = false;

    /// <summary>
    /// Gets/sets the <see cref="ProblemDetails"/> type that occurred when trying to save the resource, if any
    /// </summary>
    public Uri? ProblemType { get; set; } = null;

    /// <summary>
    /// Gets/sets the <see cref="ProblemDetails"/> title that occurred when trying to save the resource, if any
    /// </summary>
    public string ProblemTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the <see cref="ProblemDetails"/> details that occurred when trying to save the resource, if any
    /// </summary>
    public string ProblemDetail { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the <see cref="ProblemDetails"/> status that occurred when trying to save the resource, if any
    /// </summary>
    public int ProblemStatus { get; set; } = 0;

    /// <summary>
    /// Gets a <see cref="EquatableList{T}"/> that contains all <see cref="Operator"/>s
    /// </summary>
    public EquatableList<Operator>? Operators { get; set; }

    /// <summary>
    /// Gets/sets the list of <see cref="ProblemDetails"/> errors that occurred when trying to save the resource, if any
    /// </summary>
    public IDictionary<string, string[]> ProblemErrors { get; set; } = new Dictionary<string, string[]>();

}
