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

using Synapse.Api.Application.Commands.WorkflowDataDocuments;
using Synapse.Api.Application.Queries.WorkflowDataDocuments;

namespace Synapse.Api.Http.Controllers;

/// <summary>
/// Represents the <see cref="NamespacedResourceController{TResource}"/> used to manage <see cref="Workflow"/>s
/// </summary>
/// <param name="mediator">The service used to mediate calls</param>
[Route("api/v1/workflow-data")]
public class WorkflowDataController(IMediator mediator)
    : Controller
{

    /// <summary>
    /// Gets the service used to mediate calls
    /// </summary>
    protected IMediator Mediator { get; } = mediator;

    /// <summary>
    /// Creates a new workflow data document
    /// </summary>
    /// <param name="document">The document to create</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/> that describes the result of the operation</returns>
    [HttpPost]
    public async Task<IActionResult> CreateDocument([FromBody]Document document, CancellationToken cancellationToken = default)
    {
        return this.Process(await this.Mediator.ExecuteAsync(new CreateWorkflowDataDocumentCommand(document), cancellationToken), (int)HttpStatusCode.Created);
    }

    /// <summary>
    /// Gets the workflow data document with the specified id
    /// </summary>
    /// <param name="id">The id of the workflow data document to get</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/> that describes the result of the operation</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(string id, CancellationToken cancellationToken = default)
    {
        return this.Process(await this.Mediator.ExecuteAsync(new GetWorkflowDataDocumentQuery(id), cancellationToken));
    }

}
