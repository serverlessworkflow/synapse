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
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Apis.Management.Http.Controllers
{

    /// <summary>
    /// Represents the <see cref="ApiController"/> used to manage authentication definition collections
    /// </summary>
    [Tags("AuthenticationDefinitionCollections")]
    [Route("api/v1/resources/collections/authentications")]
    public class V1AuthenticationDefinitionCollectionsController
        : ApiController
    {

        /// <inheritdoc/>
        public V1AuthenticationDefinitionCollectionsController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper)
            : base(loggerFactory, mediator, mapper)
        {

        }

        /// <summary>
        /// Creates a new authentication definition collection
        /// </summary>
        /// <param name="command">An object that represents the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Integration.Models.V1AuthenticationDefinitionCollection), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> CreateWorkflow([FromBody] Integration.Commands.AuthenticationDefinitionCollections.V1CreateAuthenticationDefinitionCollectionCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.AuthenticationDefinitionCollections.V1CreateAuthenticationDefinitionCollectionCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Gets the authentication definition collection with the specified id
        /// </summary>
        /// <param name="id">The id of the authentication definition collection to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}"), EnableQuery]
        [ProducesResponseType(typeof(Integration.Models.V1AuthenticationDefinitionCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public virtual async Task<IActionResult> GetAuthenticationCollectionById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(Application.Queries.AuthenticationDefinitionCollections.V1GetAuthenticationDefinitionCollectionByIdQuery.Parse(id), cancellationToken));
        }

        /// <summary>
        /// Gets the raw authentication definition collection with the specified id
        /// </summary>
        /// <param name="id">The id of the authentication definition collection to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}/raw")]
        [ProducesResponseType(typeof(List<AuthenticationDefinition>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public virtual async Task<IActionResult> GetRawAuthenticationCollectionById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(Application.Queries.AuthenticationDefinitionCollections.V1GetRawAuthenticationDefinitionCollectionByIdQuery.Parse(id), cancellationToken));
        }

        /// <summary>
        /// Queries authentication definition collections
        /// <para>This endpoint supports ODATA.</para>
        /// </summary>
        /// <param name="queryOptions">The options of the ODATA query to perform</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet, EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<Integration.Models.V1AuthenticationDefinitionCollection>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetAuthenticationCollections(ODataQueryOptions<Integration.Models.V1AuthenticationDefinitionCollection> queryOptions, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FilterQuery<Integration.Models.V1AuthenticationDefinitionCollection>(queryOptions), cancellationToken));
        }

        /// <summary>
        /// Deletes an existing authentication definition collection
        /// </summary>
        /// <param name="id">The id of the authentication definition collection to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteAuthenticationCollection(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Generic.V1DeleteCommand<Domain.Models.V1AuthenticationDefinitionCollection, string>(id), cancellationToken));
        }

    }

}
