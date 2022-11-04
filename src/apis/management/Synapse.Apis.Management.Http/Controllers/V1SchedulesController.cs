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
    /// Represents the <see cref="ApiController"/> used to manage schedules
    /// </summary>
    [Tags("Schedules")]
    [Route("api/v1/schedules")]
    public class V1SchedulesController
        : ApiController
    {

        /// <inheritdoc/>
        public V1SchedulesController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper) : base(loggerFactory, mediator, mapper) { }

        /// <summary>
        /// Creates a new schedule
        /// </summary>
        /// <param name="command">An object that represents the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Integration.Models.V1Schedule), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> CreateSchedule([FromBody] Integration.Commands.Schedules.V1CreateScheduleCommand command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Schedules.V1CreateScheduleCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Gets the schedule with the specified id.
        /// </summary>
        /// <param name="id">The id of the schedule to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}"), EnableQuery]
        [ProducesResponseType(typeof(Integration.Models.V1Schedule), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetScheduleById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FindByIdQuery<Integration.Models.V1Schedule, string>(id), cancellationToken));
        }

        /// <summary>
        /// Queries schedules
        /// <para>This endpoint supports ODATA.</para>
        /// </summary>
        /// <param name="queryOptions">The options of the ODATA query to perform</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet, EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<Integration.Models.V1Schedule>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetSchedules(ODataQueryOptions<Integration.Models.V1Schedule> queryOptions, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Queries.Generic.V1FilterQuery<Integration.Models.V1Schedule>(queryOptions), cancellationToken));
        }

        /// <summary>
        /// Patches the specified schedule
        /// </summary>
        /// <param name="command">An object that describes the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPatch]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> PatchSchedule(Integration.Commands.Generic.V1PatchCommand<Integration.Models.V1Schedule, string> command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Generic.V1PatchCommand<Domain.Models.V1Schedule, Integration.Models.V1Schedule, string>>(command), cancellationToken));
        }

        /// <summary>
        /// Triggers a new occurence the specified schedule
        /// </summary>
        /// <param name="id">The id of the schedule to trigger</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPut("{id}/trigger")]
        [ProducesResponseType(typeof(Integration.Models.V1Schedule), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> TriggerSchedule(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Schedules.V1TriggerScheduleCommand(id), cancellationToken));
        }

        /// <summary>
        /// Suspends the specified schedule
        /// </summary>
        /// <param name="id">The id of the schedule to suspend</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPut("{id}/suspend")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> SuspendSchedule(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Schedules.V1SuspendScheduleCommand(id), cancellationToken));
        }

        /// <summary>
        /// Resumes the specified schedule
        /// </summary>
        /// <param name="id">The id of the schedule to resume</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPut("{id}/resume")]
        [ProducesResponseType(typeof(Integration.Models.V1Schedule), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> ResumeSchedule(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Schedules.V1ResumeScheduleCommand(id), cancellationToken));
        }

        /// <summary>
        /// Retires the specified schedule
        /// </summary>
        /// <param name="id">The id of the schedule to retire</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPut("{id}/retire")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> RetireSchedule(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Schedules.V1RetireScheduleCommand(id), cancellationToken));
        }

        /// <summary>
        /// Makes the specified schedule obsolete
        /// </summary>
        /// <param name="id">The id of the schedule to make obsolete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPut("{id}/obsolete")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> MakeScheduleObsolete(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Schedules.V1MakeScheduleObsoleteCommand(id), cancellationToken));
        }

        /// <summary>
        /// Deletes an existing schedule
        /// </summary>
        /// <param name="id">The id of the schedule to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteSchedule(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new Application.Commands.Generic.V1DeleteCommand<Domain.Models.V1Schedule, string>(id), cancellationToken));
        }

    }

}
