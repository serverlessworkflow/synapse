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

namespace Synapse.Apis.Management.Http.Controllers
{

    /// <summary>
    /// Represents the <see cref="ApiController"/> used to manage application metrics
    /// </summary>
    [Route("api/v1/metrics")]
    public class V1MetricsController
        : ApiController
    {

        /// <inheritdoc/>
        public V1MetricsController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper)
            : base(loggerFactory, mediator, mapper)
        {

        }

        /// <summary>
        /// Gets application metrics
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet]
        [ProducesResponseType(typeof(V1ApplicationMetrics), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FindByIdQuery<V1ApplicationMetrics, string>(V1ApplicationMetrics.GetIdFor(DateTime.Now.Date)), cancellationToken));
        }

    }

}
