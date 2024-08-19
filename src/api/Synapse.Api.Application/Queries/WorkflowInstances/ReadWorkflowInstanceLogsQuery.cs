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

using Neuroglia.Data.Infrastructure.Services;
using Synapse.Resources;

namespace Synapse.Api.Application.Queries.WorkflowInstances;

/// <summary>
/// Represents the query used to read the logs of the specified workflow instance
/// </summary>
public class ReadWorkflowInstanceLogsQuery
    : Query<string>
{

    /// <summary>
    /// Initializes a new <see cref="ReadWorkflowInstanceLogsQuery"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="WorkflowInstance"/> to read the logs of</param>
    /// <param name="namespace">The namespace the <see cref="WorkflowInstance"/> to read the logs of belongs to</param>
    public ReadWorkflowInstanceLogsQuery(string name, string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Namespace = @namespace;
    }

    /// <summary>
    /// Gets the name of the <see cref="WorkflowInstance"/> to read the logs of
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the namespace the <see cref="WorkflowInstance"/> to read the logs of belongs to
    /// </summary>
    public string? Namespace { get; }

}

/// <summary>
/// Represents the service used to handle <see cref="ReadWorkflowInstanceLogsQuery"/>s
/// </summary>
/// <param name="logDocuments">The service used to manage log documents</param>
public class ReadWorkflowInstanceLogsQueryHandler(ITextDocumentRepository<string> logDocuments)
    : IQueryHandler<ReadWorkflowInstanceLogsQuery, string>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<string>> HandleAsync(ReadWorkflowInstanceLogsQuery query, CancellationToken cancellationToken)
    {
        var logs = await logDocuments.ReadToEndAsync($"{query.Name}.{query.Namespace}", cancellationToken).ConfigureAwait(false);
        return this.Ok(logs);
    }

}
