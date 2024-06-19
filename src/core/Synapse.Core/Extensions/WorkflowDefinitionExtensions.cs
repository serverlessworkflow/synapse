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

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="WorkflowDefinition"/>s
/// </summary>
public static class WorkflowDefinitionExtensions
{

    /// <summary>
    /// Gets the task, if any, that should be executed after the specified one
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> that defines the specified <see cref="TaskDefinition"/></param>
    /// <param name="afterTask">The name/definition mapping of the <see cref="TaskDefinition"/> to get the following <see cref="TaskDefinition"/> of</param>
    /// <param name="parentReference">A reference to the component that defines the next <see cref="TaskDefinition"/></param>
    /// <returns>A name/definition mapping of the next <see cref="TaskDefinition"/>, if any</returns>
    public static MapEntry<string, TaskDefinition>? GetTaskAfter(this WorkflowDefinition workflow, MapEntry<string, TaskDefinition> afterTask, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var taskMap = workflow.GetComponent<Map<string, TaskDefinition>>(parentReference) ?? throw new NullReferenceException($"Failed to find the component at '{parentReference}'");
        var taskIndex = taskMap.Select(e => e.Key).ToList().IndexOf(afterTask.Key);
        var nextIndex = taskIndex < 0 ? -1 : taskIndex + 1;
        if (nextIndex < 0 || nextIndex >= taskMap.Count) return null;
        return taskMap.ElementAt(nextIndex);
    }

    /// <summary>
    /// Gets the index of the specified task within its defining container
    /// </summary>
    /// <param name="workflow">The extended <see cref="WorkflowDefinition"/></param>
    /// <param name="taskName">The name of the task to get the index of</param>
    /// <param name="parentReference">A reference to the parent of the task to get the index of</param>
    /// <returns>The index of the specified task</returns>
    public static int IndexOf(this WorkflowDefinition workflow, string taskName, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var taskMap = workflow.GetComponent<Map<string, TaskDefinition>>(parentReference) ?? throw new NullReferenceException($"Failed to find the component at '{parentReference}'");
        if (taskMap.TryGetValue(taskName, out var task) && task != null) return taskMap.Select(e => e.Value).ToList().IndexOf(task);
        else return -1;
    }

}