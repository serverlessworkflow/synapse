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

namespace Synapse.Runner.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ITaskExecutionContextFactory"/> interface
/// </summary>
public class TaskExecutionContextFactory
    : ITaskExecutionContextFactory
{

    /// <inheritdoc/>
    public virtual ITaskExecutionContext Create(IWorkflowExecutionContext workflow, TaskInstance task, TaskDefinition definition, object input, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(definition);
        var contextType = typeof(TaskExecutionContext<>).MakeGenericType(definition.GetType());
        return (ITaskExecutionContext)Activator.CreateInstance(contextType, workflow, task, definition, input, contextData, arguments)!;
    }

}