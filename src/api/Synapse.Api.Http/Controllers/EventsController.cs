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

using Neuroglia.Eventing.CloudEvents;
using Synapse.Api.Application.Commands.Events;

namespace Synapse.Api.Http.Controllers;

/// <summary>
/// Represents the <see cref="NamespacedResourceController{TResource}"/> used to manage <see cref="CloudEvent"/>s
/// </summary>
/// <param name="mediator">The service used to mediate calls</param>
[Route("api/v1/events")]
public class EventsController(IMediator mediator)
    : Controller
{

    /// <summary>
    /// Gets the service used to mediate calls
    /// </summary>
    protected IMediator Mediator { get; } = mediator;

    /// <summary>
    /// Publishes the specified cloud event
    /// </summary>
    /// <param name="e">The cloud event to publish</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/> that describes the result of the operation</returns>
    [HttpPost]
    public async Task<IActionResult> PublishEvent([FromBody]CloudEvent e, CancellationToken cancellationToken = default)
    {
        return this.Process(await this.Mediator.ExecuteAsync(new PublishCloudEventCommand(e)).ConfigureAwait(false), (int)HttpStatusCode.Accepted);
    }

}
