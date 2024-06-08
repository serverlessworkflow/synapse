// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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
