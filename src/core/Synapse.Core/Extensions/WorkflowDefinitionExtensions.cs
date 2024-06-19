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
    public static KeyValuePair<string, TaskDefinition>? GetNextTask(this WorkflowDefinition workflow, KeyValuePair<string, TaskDefinition> afterTask, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);
        var taskMap = workflow.GetComponent<IDictionary<string, TaskDefinition>>(parentReference) ?? throw new NullReferenceException($"Failed to find the component at '{parentReference}'");
        var taskIndex = taskMap.Keys.ToList().IndexOf(afterTask.Key);
        var nextIndex = taskIndex < 0 ? -1 : taskIndex + 1;
        if (nextIndex < 0 || nextIndex >= taskMap.Count) return null;
        return taskMap.ElementAt(nextIndex);
    }

}