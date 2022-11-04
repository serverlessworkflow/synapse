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
    /// Represents the <see cref="ApiController"/> used to manage application metrics
    /// </summary>
    [Tags("OperationalReports")]
    [Route("api/v1/reports/operations")]
    public class V1OperationalReportsController
        : ApiController
    {

        /// <inheritdoc/>
        public V1OperationalReportsController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper)
            : base(loggerFactory, mediator, mapper)
        {

        }

        /// <summary>
        /// Gets the <see cref="V1OperationalReport"/> for the specified date
        /// </summary>
        /// <param name="date">The date for which to get the <see cref="V1OperationalReport"/>. Defaults to today</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet]
        [ProducesResponseType(typeof(V1OperationalReport), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetOperationalReport(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            if (!date.HasValue)
                date = DateTime.Now;
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FindByIdQuery<V1OperationalReport, string>(V1OperationalReport.GetIdFor(date.Value)), cancellationToken));
        }

    }

}
