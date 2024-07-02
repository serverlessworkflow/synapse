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
/// Defines the fundamentals of a service used to create <see cref="ITaskExecutionContext"/>s
/// </summary>
public interface ITaskExecutionContextFactory
{

    /// <summary>
    /// Creates a new <see cref="ITaskExecutionContext"/> implementation for the <see cref="TaskInstance"/>
    /// </summary>
    /// <param name="workflow">The <see cref="IWorkflowExecutionContext"/> the <see cref="ITaskExecutionContext"/> belongs to</param>
    /// <param name="resource">The <see cref="TaskInstance"/> to run</param>
    /// <param name="definition">The <see cref="TaskDefinition"/> of the <see cref="TaskInstance"/> to run</param>
    /// <param name="input">The input, if any, of the <see cref="TaskInstance"/> to run</param>
    /// <param name="contextData">A name/value mapping of the task's context data</param>
    /// <param name="arguments">A name/value mapping of the task's arguments, if any</param>
    /// <returns>A new <see cref="ITaskExecutionContext"/></returns>
    ITaskExecutionContext Create(IWorkflowExecutionContext workflow, TaskInstance resource, TaskDefinition definition, object input, IDictionary<string, object> contextData, IDictionary<string, object>? arguments = null);

}
