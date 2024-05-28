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

namespace Synapse.Dashboard.Components.ResourceEditorStateManagement;

/// <summary>
/// Represents the state of the <see cref="ResourceEditor{TResource}"/>'s component
/// </summary>
public record ResourceEditorState<TResource>
    where TResource : Resource, new()
{

    /// <summary>
    /// Gets/sets the resource to display details about
    /// </summary>
    public TResource? Resource { get; set; } = new() { Metadata = new() { Name = "new-" + typeof(TResource).Name.ToLower() } };

    /// <summary>
    /// Gets/sets the definition of the displayed resource
    /// </summary>
    public ResourceDefinition? Definition { get; set; } = null;

    /// <summary>
    /// Gets/sets the content of the text editor
    /// </summary>
    public string TextEditorValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets a boolean indicating if the text editor is being updated
    /// </summary>
    public bool Updating { get; set; } = false;

    /// <summary>
    /// Gets/sets a boolean indicating if the resource is being saved
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
    /// Gets/sets the list of <see cref="ProblemDetails"/> errors that occurred when trying to save the resource, if any
    /// </summary>
    public IDictionary<string, string[]> ProblemErrors { get; set; } = new Dictionary<string, string[]>();

}
