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

namespace Synapse.Runtime.Services;

/// <summary>
/// Defines the fundamentals of a service used to create and run <see cref="IWorkflowProcess"/>es
/// </summary>
public interface IWorkflowRuntime
    : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Creates a new <see cref="IWorkflowProcess"/> for the specified <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="workflow">The instantiated <see cref="Workflow"/> to start a new <see cref="IWorkflowProcess"/> for</param>
    /// <param name="workflowInstance">The <see cref="WorkflowInstance"/> to create a new <see cref="IWorkflowProcess"/> for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IWorkflowProcess"/></returns>
    Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

}
