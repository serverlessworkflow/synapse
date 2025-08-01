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

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents the parameters for creating a new workflow instance.
/// </summary>
public record CreateWorkflowInstanceParameters
{
    /// <summary>
    /// Gets/sets the workflow instance input.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// Gets/sets the workflow instance's labels.
    /// </summary>
    public EquatableDictionary<string, string> Labels { get; set; } = [];

    /// <summary>
    /// Gets/sets the workflow instance's annotations.
    /// </summary>
    public EquatableDictionary<string, string> Annotations { get; set; } = [];
}
