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

namespace Synapse.Dashboard.Components;

/// <summary>
/// Defines the fundamentals of a workflow node
/// </summary>
public interface IWorkflowNodeViewModel
{

    /// <summary>
    /// Gets/Sets the number of active <see cref="WorkflowInstance"/>s for which the task described by the node is operative
    /// </summary>
    int OperativeInstancesCount { get; set; }

    /// <summary>
    /// Gets/Sets the number of active faulted <see cref="WorkflowInstance"/>s for which the task described by the node is faulted
    /// </summary>
    int FaultedInstancesCount { get; set; }

    /// <summary>
    /// Resets the operative and faulted instances counts
    /// </summary>
    void ResetInstancesCount();

}
