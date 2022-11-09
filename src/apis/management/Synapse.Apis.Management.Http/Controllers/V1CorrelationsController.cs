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

using Microsoft.AspNetCore.Http;

namespace Synapse.Apis.Management.Http.Controllers
{

    /// <summary>
    /// Represents the <see cref="ApiController"/> used to manage correlation definitions
    /// </summary>
    [Tags("Correlations")]
    [Route("api/v1/correlations")]
    public class V1CorrelationsController
        : ApiController
    {

        /// <inheritdoc/>
        public V1CorrelationsController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper)
            : base(loggerFactory, mediator, mapper)
        {

        }

        /// <summary>
        /// Creates a new correlation
        /// </summary>
        /// <param name="command">An object that represents the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Integration.Models.V1Correlation), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> CreateCorrelation([FromBody] Integration.Commands.Correlations.V1CreateCorrelationCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Correlations.V1CreateCorrelationCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Gets the correlation with the specified id.
        /// </summary>
        /// <param name="id">The id of the correlation to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}"), EnableQuery]
        [ProducesResponseType(typeof(Integration.Models.V1Correlation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetCorrelationById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FindByIdQuery<Integration.Models.V1Correlation, string>(id), cancellationToken));
        }

        /// <summary>
        /// Queries correlations
        /// <para>This endpoint supports ODATA.</para>
        /// </summary>
        /// <param name="queryOptions">The options used to configure the query to perform</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet, EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<Integration.Models.V1Correlation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetCorrelations(ODataQueryOptions<Integration.Models.V1Correlation> queryOptions, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FilterQuery<Integration.Models.V1Correlation>(queryOptions), cancellationToken));
        }

        /// <summary>
        /// Patches an existing correlation
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
        public async Task<IActionResult> PatchCorrelation([FromBody] Integration.Commands.Generic.V1PatchCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Generic.V1PatchCommand<Domain.Models.V1Correlation, Integration.Models.V1Correlation, string >>(command), cancellationToken));
        }

        /// <summary>
        /// Deletes an existing correlation context
        /// </summary>
        /// <param name="correlationId">The id of the correlation the context to delete belongs to</param>
        /// <param name="contextId">The id of the correlation context to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("byid/{correlationId}/context/{contextId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteCorrelationContext(string correlationId, string contextId, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Correlations.V1DeleteCorrelationContextCommand(correlationId, contextId), cancellationToken), (int)HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a correlated event
        /// </summary>
        /// <param name="correlationId">The id of the correlation the correlated event to delete belongs to</param>
        /// <param name="contextId">The id of the context the correlated event to delete belongs to</param>
        /// <param name="eventId">The id of the correlated event to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("byid/{correlationId}/context/{contextId}/events/{eventId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteCorrelatedEvent(string correlationId, string contextId, string eventId, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Correlations.V1DeleteCorrelatedEventCommand(correlationId, contextId, eventId), cancellationToken), (int)HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes an existing correlation
        /// </summary>
        /// <param name="id">The id of the correlation to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("byid/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteCorrelation(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Generic.V1DeleteCommand<Domain.Models.V1Correlation, string>(id), cancellationToken), (int)HttpStatusCode.NoContent);
        }

    }

}
