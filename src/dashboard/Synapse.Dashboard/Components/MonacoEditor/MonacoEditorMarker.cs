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

/// <inheritdoc />
public class MonacoEditorMarker 
    : IMonacoEditorMarker
{
    /// <inheritdoc />
    public string? Code { get; set; }
    /// <inheritdoc />
    public int EndColumn { get; set; }
    /// <inheritdoc />
    public int EndLineNumber { get; set; }
    /// <inheritdoc />
    public string Message { get; set; } = "";
    /// <inheritdoc />
    public string Owner { get; set; } = "";
    /// <inheritdoc />
    public IEnumerable<Object>? RelatedInformation { get; set; }
    /// <inheritdoc />
    public Object Resource { get; set; } = "";
    /// <inheritdoc />
    public int Severity { get; set; }
    /// <inheritdoc />
    public int StartColumn { get; set; }
    /// <inheritdoc />
    public int StartLineNumber { get; set; }
    /// <inheritdoc />
    public IEnumerable<Object>? Tags { get; set; }
    /// <inheritdoc />
    public string? ModelVersionId { get; set; }
    /// <inheritdoc />
    public string? Source { get; set; }
}
