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

using Neuroglia.Data.Infrastructure;

namespace Synapse.Api.Client.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage <see cref="WorkflowInstance"/>s using the Synapse API
/// </summary>
public interface IWorkflowInstanceApiClient
    : INamespacedResourceApiClient<WorkflowInstance>
{

    /// <summary>
    /// Reads the logs of the specified <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="WorkflowInstance"/> to read the logs of</param>
    /// <param name="namespace">The namespace the <see cref="WorkflowInstance"/> to read the logs of belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The logs of the specified <see cref="WorkflowInstance"/></returns>
    Task<string> ReadLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default);

    /// <summary>
    /// Watches the logs of the specified <see cref="WorkflowInstance"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="WorkflowInstance"/> to watch the logs of</param>
    /// <param name="namespace">The namespace the <see cref="WorkflowInstance"/> to watch the logs of belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to watch the logs of the specified <see cref="WorkflowInstance"/></returns>
    Task<IAsyncEnumerable<ITextDocumentWatchEvent>> WatchLogsAsync(string name, string @namespace, CancellationToken cancellationToken = default);

}
