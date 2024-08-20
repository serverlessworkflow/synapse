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

using Neuroglia.Blazor.Dagre;

namespace Synapse.Dashboard.Components;

/// <summary>
/// Represents the object that holds the data required to render the view of a workflow's start node 
/// </summary>
public class StartNodeViewModel(bool hasSuccessor = false)
    : WorkflowNodeViewModel("start-node", new() { CssClass = "start-node", Shape = NodeShape.Circle, Width = WorkflowGraphBuilder.StartEndNodeRadius, Height = WorkflowGraphBuilder.StartEndNodeRadius })
{

    /// <summary>
    /// Gets a boolean indicating whether or not the node has a successor
    /// </summary>
    public bool HasSuccessor { get; } = hasSuccessor;

}