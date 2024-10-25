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
using Synapse.Api.Application.Commands.WorkflowInstances;
using Synapse.Api.Application.Queries.WorkflowInstances;

namespace Synapse.Api.Http.Controllers;

/// <summary>
/// Represents the <see cref="NamespacedResourceController{TResource}"/> used to manage <see cref="Workflow"/>s
/// </summary>
/// <param name="mediator">The service used to mediate calls</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
[Route("api/v1/workflow-instances")]
public class WorkflowInstancesController(IMediator mediator, IJsonSerializer jsonSerializer)
    : NamespacedResourceController<WorkflowInstance>(mediator, jsonSerializer)
{

    /// <summary>
    /// Suspends the execution of the specified workflow instance
    /// </summary>
    /// <param name="name">The name of the workflow instance to suspend</param>
    /// <param name="namespace">The namespace the workflow instance to suspend belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPut("{namespace}/{name}/suspend")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> SuspendWorkflowInstance(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        if(!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new SuspendWorkflowInstanceCommand(name, @namespace)).ConfigureAwait(false));
    }

    /// <summary>
    /// Resumes the execution of the specified workflow instance
    /// </summary>
    /// <param name="name">The name of the workflow instance to resume</param>
    /// <param name="namespace">The namespace the workflow instance to resume belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPut("{namespace}/{name}/resume")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> ResumeWorkflowInstance(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new ResumeWorkflowInstanceCommand(name, @namespace)).ConfigureAwait(false));
    }

    /// <summary>
    /// Cancels the execution of the specified workflow instance
    /// </summary>
    /// <param name="name">The name of the workflow instance to cancel</param>
    /// <param name="namespace">The namespace the workflow instance to cancel belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpPut("{namespace}/{name}/cancel")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> CancelWorkflowInstance(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new CancelWorkflowInstanceCommand(name, @namespace)).ConfigureAwait(false));
    }

    /// <summary>
    /// Gets the logs produced by workflow instance with the the specified name and namespace
    /// </summary>
    /// <param name="name">The name of the workflow instance to read the logs of</param>
    /// <param name="namespace">The namespace the workflow instance to read the logs of belongs to</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("{namespace}/{name}/logs")]
    [ProducesResponseType(typeof(Resource), (int)HttpStatusCode.Created)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public async Task<IActionResult> GetWorkflowInstanceLogs(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) return this.ValidationProblem(this.ModelState);
        return this.Process(await this.Mediator.ExecuteAsync(new ReadWorkflowInstanceLogsQuery(name, @namespace), cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Watches the logs of a specific workflow instance
    /// </summary>
    /// <param name="namespace">The namespace the workflow instance to watch the logs of belongs to</param>
    /// <param name="name">The name of the workflow instance to watch the logs of</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IActionResult"/></returns>
    [HttpGet("{namespace}/{name}/logs/watch")]
    [ProducesResponseType(typeof(IAsyncEnumerable<ResourceWatchEvent>), (int)HttpStatusCode.OK)]
    [ProducesErrorResponseType(typeof(Neuroglia.ProblemDetails))]
    public virtual async Task<IAsyncEnumerable<ITextDocumentWatchEvent>> WatchWorkflowInstanceLogs(string name, string @namespace, CancellationToken cancellationToken = default)
    {
        var response = await this.Mediator.ExecuteAsync(new WatchWorkflowInstanceLogsQuery(name, @namespace), cancellationToken).ConfigureAwait(false);
        return response.Data!;
    }

}