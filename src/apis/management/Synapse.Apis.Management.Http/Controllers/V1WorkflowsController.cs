/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System.Net.Mime;

namespace Synapse.Apis.Management.Http.Controllers
{

    /// <summary>
    /// Represents the <see cref="ApiController"/> used to manage workflow definitions
    /// </summary>
    [Route("api/v1/workflows")]
    public class V1WorkflowsController
        : ApiController
    {

        /// <inheritdoc/>
        public V1WorkflowsController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper) 
            : base(loggerFactory, mediator, mapper)
        {

        }

        /// <summary>
        /// Creates a new workflow
        /// </summary>
        /// <param name="command">An object that represents the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Integration.Models.V1Workflow), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Create([FromBody] Integration.Commands.Workflows.V1CreateWorkflowCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Workflows.V1CreateWorkflowCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Uploads a new workflow
        /// </summary>
        /// <param name="command">An object that represents the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(Integration.Models.V1Workflow), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Upload([FromForm] Integration.Commands.Workflows.V1UploadWorkflowCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Workflows.V1UploadWorkflowCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Gets the workflow with the specified id.
        /// </summary>
        /// <param name="id">The id of the workflow to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}"), EnableQuery]
        [ProducesResponseType(typeof(Integration.Models.V1Workflow), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(Application.Queries.Workflows.V1GetWorkflowByIdQuery.Parse(id), cancellationToken));
        }

        /// <summary>
        /// Gets the workflow instance with the specified id.
        /// </summary>
        /// <param name="id">The id of the workflow of the instance to get</param>
        /// <param name="instanceKey">The key of the specified workflow's instance to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}/{instanceKey}"), EnableQuery]
        [ProducesResponseType(typeof(Integration.Models.V1WorkflowInstance), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetInstanceByKey(string id, string instanceKey, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FindByIdQuery<Integration.Models.V1WorkflowInstance, string>($"{id}-{instanceKey}"), cancellationToken));
        }

        /// <summary>
        /// Queries workflows
        /// <para>This endpoint supports ODATA.</para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet, EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<Integration.Models.V1Workflow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1ListQuery<Integration.Models.V1Workflow>(), cancellationToken));
        }

        /// <summary>
        /// Patches an existing workflow
        /// </summary>
        /// <param name="command">An object used to describe the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPatch]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Patch([FromBody] Integration.Commands.Generic.V1PatchCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Generic.V1PatchCommand<Domain.Models.V1Workflow, string>>(command), cancellationToken));
        }

        /// <summary>
        /// Donwloads the archive of an existing workflow
        /// </summary>
        /// <param name="id">The id of the workflow to archive</param>
        /// <param name="version">The version of the workflow to archive</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}/archive")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Archive(string id, string? version, CancellationToken cancellationToken)
        {
            var result = await this.Mediator.ExecuteAsync(new Application.Commands.Workflows.V1ArchiveWorkflowCommand(id, version), cancellationToken);
            if (!result.Succeeded)
                return this.Process(result);
            return this.File(result.Data, MediaTypeNames.Application.Zip, $"{id}.zip");
        }

        /// <summary>
        /// Deletes an existing workflow
        /// </summary>
        /// <param name="id">The id of the workflow to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Workflows.V1DeleteWorkflowCommand(id), cancellationToken));
        }

    }

}
