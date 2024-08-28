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

namespace Synapse.Dashboard.Pages.Workflows.List;

/// <summary>
/// Represents the <see cref="View"/>'s state
/// </summary>
public record WorkflowListState
    : NamespacedResourceManagementComponentState<Workflow>
{
    /// <summary>
    /// Gets a <see cref="EquatableList{T}"/> that contains all <see cref="Operator"/>s
    /// </summary>
    public EquatableList<Operator>? Operators { get; set; }

    /// <summary>
    /// Gets/sets the active operator filter
    /// </summary>
    public string? Operator { get; set; } = null;
}