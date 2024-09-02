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

using Synapse.Api.Application.Commands.Resources.Generic;
using Synapse.Api.Application.Queries.Resources.Generic;

namespace Synapse.Api.Http;

/// <summary>
/// Represents the base class of a <see cref="Controller"/> used to manage <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
/// <param name="mediator">The service used to mediate calls</param>
public abstract class ResourceController<TResource>(IMediator mediator)
    : Controller
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Gets the service used to mediate calls
    /// </summary>
    protected IMediator Mediator { get; } = mediator;

    /// <summary>
    /// Creates a new resource of the specified type
    /// </summary>
    /// <param name="resource">The resource to create</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPost]
    [ProducesResponseType(typeof(Resource), (int)HttpStatusCode.Created)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> CreateResource([FromBody] TResource resource, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new CreateResourceCommand<TResource>(resource), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Gets the definition of the managed resources
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("definition")]
    [ProducesResponseType(typeof(ResourceDefinition), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> GetResourceDefinition(CancellationToken cancellationToken = default)
    {
        return this.Process(await this.Mediator.ExecuteAsync(new GetResourceDefinitionQuery<TResource>(), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Replaces the specified resource
    /// </summary>
    /// <param name="resource">The resource to replace</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPut]
    [ProducesResponseType(typeof(IAsyncEnumerable<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> ReplaceResource(TResource resource, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new ReplaceResourceCommand<TResource>(resource), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Replaces the status of the specified resource
    /// </summary>
    /// <param name="resource">The resource to replace</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPut]
    [ProducesResponseType(typeof(IAsyncEnumerable<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> ReplaceResourceStatus(TResource resource, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new ReplaceResourceStatusCommand<TResource>(resource), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Parses the specified string into a new <see cref="List{T}"/> of <see cref="LabelSelector"/>s
    /// </summary>
    /// <param name="labelSelector">The string to parse</param>
    /// <param name="labelSelectors">A new <see cref="List{T}"/> containing the parsed <see cref="LabelSelector"/>s</param>
    /// <returns>A boolean indicating whether or not the input could be parse</returns>
    protected virtual bool TryParseLabelSelectors(string? labelSelector, out IEnumerable<LabelSelector>? labelSelectors)
    {
        labelSelectors = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(labelSelector)) labelSelectors = LabelSelector.ParseList(labelSelector);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new <see cref="IActionResult"/> that describes an error while parsing the request's label selector
    /// </summary>
    /// <param name="labelSelector">The invalid label selector</param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    protected IActionResult InvalidLabelSelector(string labelSelector)
    {
        this.ModelState.AddModelError(nameof(labelSelector), $"The specified value '{labelSelector}' is not a valid comma-separated label selector list");
        return this.ValidationProblem("Bad Request", statusCode: (int)HttpStatusCode.BadRequest, title: "Bad Request", modelStateDictionary: this.ModelState);
    }

}
