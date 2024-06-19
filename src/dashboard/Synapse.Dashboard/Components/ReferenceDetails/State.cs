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

namespace Synapse.Dashboard.Components.ReferenceDetailsStateManagement;

/// <summary>
/// Represents the state of a <see cref="Synapse.Dashboard.Components.ReferenceDetails"/>
/// </summary>
public record ReferenceDetailsState
{

    /// <summary>
    /// Gets/sets the label to display
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the reference to load
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the referenced document
    /// </summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets a boolean indicating the reference has been loaded
    /// </summary>
    public bool Loaded { get; set; } = false;

    /// <summary>
    /// Gets/sets the <see cref="StandaloneCodeEditor"/> used to display the referenced document
    /// </summary>
    public StandaloneCodeEditor? TextEditor { get; set; } = null;

    /// <summary>
    /// Gets/sets the <see cref="TextModel"/> used by the <see cref="StandaloneCodeEditor"/>
    /// </summary>
    public TextModel? TextModel { get; set; } = null;

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
    /// Gets/sets the list of <see cref="ProblemDetails"/> errors that occurred when trying to save the resource, if any
    /// </summary>
    public IDictionary<string, string[]> ProblemErrors { get; set; } = new Dictionary<string, string[]>();
}
