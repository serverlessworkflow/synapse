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

namespace Synapse.Dashboard.Components.WorkflowInstanceLogsStateManagement;

/// <summary>
/// Represents the state of a <see cref="WorkflowInstanceLogsState"/>
/// </summary>
public record WorkflowInstanceLogsState
{
    /// <summary>
    /// Gets/sets the <see cref="WorkflowInstance"/>'s name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the <see cref="WorkflowInstance"/>'s namespace
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets a boolean indicating if the logs have been loaded
    /// </summary>
    public bool Loaded { get; set; } = false;

    /// <summary>
    /// Gets/sets the <see cref="WorkflowInstance"/>'s logs
    /// </summary>
    public IEnumerable<string> Logs { get; set; } = [];
}
