namespace Synapse.Correlator.Controllers;

/// <summary>
/// Represents the service used to manage <see cref="CloudEvent"/>s
/// </summary>
/// <param name="mediator">The service used to mediate calls</param>
[Route("api/v1/events")]
public class CloudEventsController(IMediator mediator)
    : Controller
{

    /// <summary>
    /// Gets the service used to mediate calls
    /// </summary>
    protected IMediator Mediator { get; } = mediator;

    /// <summary>
    /// Publishes the specified <see cref="CloudEvent"/> to the correlator
    /// </summary>
    /// <param name="e">The <see cref="CloudEvent"/> to publish</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/> that describes the result of the operation</returns>
    [HttpPost("pub")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesDefaultResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    public async Task<IActionResult> Pub([FromBody] CloudEvent e, CancellationToken cancellationToken)
    {
        var result = await this.Mediator.ExecuteAsync(new IngestCloudEventCommand(e), cancellationToken).ConfigureAwait(false);
        return this.Process(result, (int)HttpStatusCode.Accepted);
    }
}
