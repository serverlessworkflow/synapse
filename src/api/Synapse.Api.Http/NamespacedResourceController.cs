using Synapse.Api.Application.Commands.Resources.Generic;
using Synapse.Api.Application.Queries.Resources.Generic;
using Neuroglia.Data;

namespace Synapse.Api.Http;

/// <summary>
/// Represents the base class of a <see cref="Controller"/> used to manage namespaced <see cref="IResource"/>s
/// </summary>
/// <typeparam name="TResource">The type of <see cref="IResource"/> to manage</typeparam>
/// <param name="mediator">The service used to mediate calls</param>
public abstract class NamespacedResourceController<TResource>(IMediator mediator)
    : ResourceController<TResource>(mediator)
    where TResource : class, IResource, new()
{

    /// <summary>
    /// Gets the resource with the specified name and namespace
    /// </summary>
    /// <param name="name">The name of the resource to get</param>
    /// <param name="namespace">The namespace the resource to get belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("{namespace}/{name}")]
    [ProducesResponseType(typeof(Resource), (int)HttpStatusCode.Created)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> GetResource(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new GetResourceQuery<TResource>(name, @namespace), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Lists matching resources
    /// </summary>
    /// <param name="labelSelector">A comma-separated list of label selectors, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Collection<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> GetResources(string? labelSelector = null, CancellationToken cancellationToken = default)
    {
        if (!this.TryParseLabelSelectors(labelSelector, out var labelSelectors)) return this.InvalidLabelSelector(labelSelector!);
        return this.Process(await this.Mediator.ExecuteAsync(new GetResourcesQuery<TResource>(null, labelSelectors), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Lists matching resources
    /// </summary>
    /// <param name="namespace">The namespace to resources to list belong to</param>
    /// <param name="labelSelector">A comma-separated list of label selectors, if any</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("{namespace}")]
    [ProducesResponseType(typeof(Collection<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> GetResources(string @namespace, string? labelSelector = null, CancellationToken cancellationToken = default)
    {
        if (!this.TryParseLabelSelectors(labelSelector, out var labelSelectors)) return this.InvalidLabelSelector(labelSelector!);
        return this.Process(await this.Mediator.ExecuteAsync(new GetResourcesQuery<TResource>(@namespace, labelSelectors), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Lists matching resources
    /// </summary>
    /// <param name="labelSelector">A comma-separated list of label selectors, if any</param>
    /// <param name="maxResults">The maximum amount, if any, of results to list at once</param>
    /// <param name="continuationToken">A token, defined by a previously retrieved collection, used to continue enumerating through matches</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(Collection<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> ListResources(string? labelSelector = null, ulong? maxResults = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        if (!this.TryParseLabelSelectors(labelSelector, out var labelSelectors)) return this.InvalidLabelSelector(labelSelector!);
        return this.Process(await this.Mediator.ExecuteAsync(new ListResourcesQuery<TResource>(null, labelSelectors, maxResults, continuationToken), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Lists matching resources
    /// </summary>
    /// <param name="namespace">The namespace to resources to list belong to</param>
    /// <param name="labelSelector">A comma-separated list of label selectors, if any</param>
    /// <param name="maxResults">The maximum amount, if any, of results to list at once</param>
    /// <param name="continuationToken">A token, defined by a previously retrieved collection, used to continue enumerating through matches</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("{namespace}/list")]
    [ProducesResponseType(typeof(Collection<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> ListResources(string @namespace, string? labelSelector = null, ulong? maxResults = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        if (!this.TryParseLabelSelectors(labelSelector, out var labelSelectors)) return this.InvalidLabelSelector(labelSelector!);
        return this.Process(await this.Mediator.ExecuteAsync(new ListResourcesQuery<TResource>(@namespace, labelSelectors, maxResults, continuationToken), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Patches the specified resource
    /// </summary>
    /// <param name="name">The name of the resource to patch</param>
    /// <param name="namespace">The namespace the resource to patch belongs to</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPatch("{namespace}/{name}")]
    [ProducesResponseType(typeof(IAsyncEnumerable<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> PatchResource(string name, string @namespace, [FromBody] Patch patch, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new PatchResourceCommand<TResource>(name, @namespace, patch), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Patches the specified resource's status
    /// </summary>
    /// <param name="name">The name of the resource to patch the status of</param>
    /// <param name="namespace">The namespace the resource to patch the status of belongs to</param>
    /// <param name="patch">The patch to apply</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPatch("{namespace}/{name}/status")]
    [ProducesResponseType(typeof(IAsyncEnumerable<Resource>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IActionResult> PatchResourceStatus(string name, string @namespace, [FromBody] Patch patch, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new PatchResourceStatusCommand<TResource>(name, @namespace, patch), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Deletes the resource with the specified name and namespace
    /// </summary>
    /// <param name="name">The name of the resource to delete</param>
    /// <param name="namespace">The namespace the delete to get belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpDelete("{namespace}/{name}")]
    [ProducesResponseType(typeof(Resource), (int)HttpStatusCode.Created)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> DeleteResource(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new DeleteResourceCommand<TResource>(name, @namespace), cancellationToken).ConfigureAwait(false));
    }

}
