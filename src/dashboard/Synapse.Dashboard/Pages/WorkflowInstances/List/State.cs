﻿// Copyright © 2024-Present The Synapse Authors
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

namespace Synapse.Dashboard.Pages.WorkflowInstances.List;

/// <summary>
/// Represents the <see cref="View"/>'s state
/// </summary>
public record WorkflowInstanceListState
    : NamespacedResourceManagementComponentState<WorkflowInstance>
{

    /// <summary>
    /// Gets/sets a <see cref="List{T}"/> that contains all cached <see cref="Workflow"/>s
    /// </summary>
    public EquatableList<Workflow>? Workflows { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="Resources.Workflow"/>, if any, to filter by the instances to list 
    /// </summary>
    public Workflow? Workflow { get; set; }

    /// <summary>
    /// Gets/sets the versions of the selected <see cref="Resources.Workflow"/>, if any
    /// </summary>
    public EquatableList<string>? Versions { get; set; }

    /// <summary>
    /// Gets/sets the version of the <see cref="Resources.Workflow"/> to filter by the instances to list
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets a <see cref="EquatableList{T}"/> that contains all <see cref="Operator"/>s
    /// </summary>
    public EquatableList<Operator>? Operators { get; set; }

    /// <summary>
    /// Gets/sets the active operator filter
    /// </summary>
    public string? Operator { get; set; } = null;

}