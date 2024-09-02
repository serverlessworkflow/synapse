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
using Neuroglia.Data.Infrastructure.Services;
using Synapse.Resources;

namespace Synapse.Api.Application.Queries.WorkflowInstances;

/// <summary>
/// Represents the query used to watch the logs of a specified <see cref="WorkflowInstance"/>
/// </summary>
public class WatchWorkflowInstanceLogsQuery
    : Query<IAsyncEnumerable<ITextDocumentWatchEvent>>
{

    /// <summary>
    /// Initializes a new <see cref="WatchWorkflowInstanceLogsQuery"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="WorkflowInstance"/> to watch the logs of</param>
    /// <param name="namespace">The namespace the <see cref="WorkflowInstance"/> to watch the logs of belongs to</param>
    public WatchWorkflowInstanceLogsQuery(string name, string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Namespace = @namespace;
    }

    /// <summary>
    /// Gets the name of the <see cref="WorkflowInstance"/> to watch the logs of
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the namespace the <see cref="WorkflowInstance"/> to watch the logs of belongs to
    /// </summary>
    public string? Namespace { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="WatchWorkflowInstanceLogsQuery"/>s
/// </summary>
/// <param name="logDocuments">The service used to manage log documents</param>
public class WatchWorkflowInstanceLogsQueryHandler(ITextDocumentRepository<string> logDocuments)
    : IQueryHandler<WatchWorkflowInstanceLogsQuery, IAsyncEnumerable<ITextDocumentWatchEvent>>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<IAsyncEnumerable<ITextDocumentWatchEvent>>> HandleAsync(WatchWorkflowInstanceLogsQuery query, CancellationToken cancellationToken)
    {
        var logs = await logDocuments.WatchAsync($"{query.Name}.{query.Namespace}", cancellationToken).ConfigureAwait(false);
        return this.Ok(logs.ToAsyncEnumerable());
    }

}