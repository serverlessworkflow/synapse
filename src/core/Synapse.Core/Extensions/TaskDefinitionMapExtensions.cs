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

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="Map{TKey, TValue}"/> of <see cref="TaskDefinition"/>s
/// </summary>
public static class TaskDefinitionMapExtensions
{

    /// <summary>
    /// Gets the task, if any, that should be executed after the specified one
    /// </summary>
    /// <param name="tasks">The map that defines the <see cref="TaskDefinition"/> to get the next <see cref="TaskDefinition"/> for</param>
    /// <param name="taskName">The name of the <see cref="TaskDefinition"/> to get the next <see cref="TaskDefinition"/> for</param>
    /// <returns>A name/definition mapping of the next <see cref="TaskDefinition"/>, if any</returns>
    public static MapEntry<string, TaskDefinition>? GetTaskAfter(this Map<string, TaskDefinition> tasks, string taskName)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);
        var index = tasks.Select(t => t.Key).ToList().IndexOf(taskName);
        if (index == -1 || index + 1 >= tasks.Count) return null;
        index++;
        return tasks.ElementAt(index);
    }

    /// <summary>
    /// Gets the task, if any, that should be executed after the specified one
    /// </summary>
    /// <param name="tasks">The map that defines the <see cref="TaskDefinition"/> to get the next <see cref="TaskDefinition"/> for</param>
    /// <param name="task">The <see cref="TaskInstance"/> to get the next <see cref="TaskDefinition"/> for</param>
    /// <returns>A name/definition mapping of the next <see cref="TaskDefinition"/>, if any</returns>
    public static MapEntry<string, TaskDefinition>? GetTaskAfter(this Map<string, TaskDefinition> tasks, TaskInstance task)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        ArgumentNullException.ThrowIfNull(task);
        return (task.Status == TaskInstanceStatus.Skipped ? FlowDirective.Continue : task.Next) switch
        {
            FlowDirective.Continue or null => tasks.GetTaskAfter(task.Name!),
            FlowDirective.End or FlowDirective.Exit => null,
            _ => tasks.GetEntry(task.Next!)
        };
    }

}