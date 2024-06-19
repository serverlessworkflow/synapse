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

using ServerlessWorkflow.Sdk.Models.Tasks;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents a extension task node view model
/// </summary>
public class ExtensionTaskNodeViewModel
    : LabeledWorkflowNodeViewModel
{

    /// <summary>
    /// Initializes a new <see cref="ExtensionTaskNodeViewModel"/>
    /// </summary>
    public ExtensionTaskNodeViewModel(KeyValuePair<string, ExtensionTaskDefinition> task)
        : base(task.Key, "composite-task-node", null, Neuroglia.Blazor.Dagre.Constants.NodeHeight * 1.5, Neuroglia.Blazor.Dagre.Constants.NodeHeight * 1.5)
    {

    }

}