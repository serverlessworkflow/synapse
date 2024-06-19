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

using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard.Services;

/// <summary>
/// Defines the fundamentals of a service used to build workflow graphs
/// </summary>
public interface IWorkflowGraphBuilder
{

    /// <summary>
    /// Builds a new workflow <see cref="IGraphViewModel"/>
    /// </summary>
    /// <param name="workflow">The <see cref="WorkflowDefinition"/> to build a new <see cref="IGraphViewModel"/> for</param>
    /// <returns>A new <see cref="IGraphViewModel"/></returns>
    Task<IGraphViewModel> Build(WorkflowDefinition workflow);

}
