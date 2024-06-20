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

using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using System.Reflection;

namespace Synapse.Dashboard.Services;

public static class DebugWorkflowExtensions
{
    public static MapEntry<string, TaskDefinition>? GetTaskAfter2(this WorkflowDefinition workflow, MapEntry<string, TaskDefinition> afterTask, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);

        var taskMap = workflow.GetComponent2<Map<string, TaskDefinition>>(parentReference) ?? throw new NullReferenceException($"Failed to find the component at '{parentReference}'");

        // Directly find the index of the afterTask.Key
        int taskIndex = -1;
        for (int i = 0; i < taskMap.Count; i++)
        {
            if (taskMap.ElementAt(i).Key == afterTask.Key)
            {
                taskIndex = i;
                break;
            }
        }

        var nextIndex = taskIndex + 1;
        if (taskIndex == -1 || nextIndex >= taskMap.Count)
        {
            return null;
        }

        return taskMap.ElementAt(nextIndex);
    }

    public static int IndexOf2(this WorkflowDefinition workflow, string taskName, string parentReference)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentReference);

        var taskMap = workflow.GetComponent2<Map<string, TaskDefinition>>(parentReference) ?? throw new NullReferenceException($"Failed to find the component at '{parentReference}'");

        // Directly find the index of the task by its name
        int index = 0;
        foreach (var entry in taskMap)
        {
            if (entry.Key == taskName)
            {
                return index;
            }
            index++;
        }

        return -1;
    }

    public static TComponent GetComponent2<TComponent>(this WorkflowDefinition workflow, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var properties = path.Split('.');
        object currentObject = workflow;

        foreach (var property in properties)
        {
            var propInfo = currentObject.GetType().GetProperty(property, BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo == null)
            {
                throw new NullReferenceException($"Failed to find a component definition of type '{typeof(TComponent).Name}' at '{path}'");
            }

            currentObject = propInfo.GetValue(currentObject);
            if (currentObject == null)
            {
                throw new NullReferenceException($"Failed to find a component definition of type '{typeof(TComponent).Name}' at '{path}'");
            }
        }

        if (currentObject is TComponent component)
        {
            return component;
        }

        throw new InvalidCastException($"Component at '{path}' is not of type '{typeof(TComponent).Name}'");
    }
}
