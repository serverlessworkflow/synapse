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

namespace Synapse.Dashboard.Components;


/// <summary>
/// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html
/// </summary>
public interface IMonacoEditorMarker
{
    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#endColumn
    /// </summary>
    public int EndColumn { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#endLineNumber
    /// </summary>
    public int EndLineNumber { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#modelVersionId
    /// </summary>
    public string? ModelVersionId { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#owner
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#relatedInformation
    /// </summary>
    public IEnumerable<Object>? RelatedInformation { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#resource
    /// </summary>
    public Object Resource { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#severity
    /// </summary>
    public int Severity { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#source
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#startColumn
    /// </summary>
    public int StartColumn  { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#startLineNumber
    /// </summary>
    public int StartLineNumber { get; set; }

    /// <summary>
    /// See https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IMarker.html#tags
    /// </summary>
    public IEnumerable<Object>? Tags { get; set; }
}
