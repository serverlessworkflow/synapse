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
using Neuroglia.Data.Infrastructure.Services;

namespace Synapse.Api.Application.Queries.WorkflowDataDocuments;

/// <summary>
/// Represent the <see cref="IQuery"/> used to get a specific workflow data document
/// </summary>
/// <param name="id">The id of the workflow data document to get</param>
public class GetWorkflowDataDocumentQuery(string id)
    : Query<Document>
{

    /// <summary>
    /// Gets the id of the workflow data document to get
    /// </summary>
    public string Id { get; } = id;

}

/// <summary>
/// Represents the service used to handle <see cref="GetWorkflowDataDocumentQuery"/> instances
/// </summary>
/// <param name="documents">The <see cref="IRepository"/> used to manage workflow data <see cref="Document"/>s</param>
public class GetWorkflowDataQueryHandler(IRepository<Document> documents)
    : IQueryHandler<GetWorkflowDataDocumentQuery, Document>
{

    /// <inheritdoc/>
    public virtual async Task<IOperationResult<Document>> HandleAsync(GetWorkflowDataDocumentQuery query, CancellationToken cancellationToken)
    {
        return this.Ok(await documents.GetAsync(query.Id, cancellationToken).ConfigureAwait(false));
    }

}
