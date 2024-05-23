using Synapse.Api.Application.Commands.WorkflowDataDocuments;
using Synapse.Api.Application.Queries.WorkflowDataDocuments;
using Synapse.Resources;

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
