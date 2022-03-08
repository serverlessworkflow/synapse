﻿using Synapse.Application.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Generic;
using Synapse.Integration.Commands.WorkflowInstances;

namespace Synapse.Ports.HttpRest.Controllers
{

    /// <summary>
    /// Represents the <see cref="ApiController"/> used to manage workflow definitions
    /// </summary>
    [Route("api/v1/workflow-instances")]
    public class V1WorkflowInstancesController
        : ApiController
    {

        /// <inheritdoc/>
        public V1WorkflowInstancesController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper)
            : base(loggerFactory, mediator, mapper)
        {

        }

        /// <summary>
        /// Creates a new workflow instance
        /// </summary>
        /// <param name="command">An object that represents the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(V1WorkflowInstanceDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Create([FromBody] V1CreateWorkflowInstanceCommandDto command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1CreateWorkflowInstanceCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Gets the workflow instance with the specified id.
        /// </summary>
        /// <param name="id">The id of the workflow instance to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}"), EnableQuery]
        [ProducesResponseType(typeof(V1WorkflowInstanceDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1WorkflowInstanceDto, string>(id), cancellationToken));
        }

        /// <summary>
        /// Queries workflow instances
        /// <para>This endpoint supports ODATA.</para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet, EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<V1WorkflowInstanceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1ListQuery<V1WorkflowInstanceDto>(), cancellationToken));
        }

        //------------------------------------------------------------
        //todo: replace with a better flow:

        [HttpPut("byid/{id}/schedule")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Schedule(string id, DateTimeOffset at, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1ScheduleWorkflowInstanceCommand(id, at), cancellationToken));
        }

        [HttpPut("byid/{id}/start")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Start(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1StartWorkflowInstanceCommand(id), cancellationToken));
        }

        //------------------------------------------------------------

        /// <summary>
        /// Patches an existing workflow instance
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
        public async Task<IActionResult> Patch([FromBody] V1PatchCommandDto command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1PatchCommand<V1WorkflowInstance, string>>(command), cancellationToken));
        }

        /// <summary>
        /// Deletes an existing workflow instance
        /// </summary>
        /// <param name="id">The id of the workflow definition to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("byid/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1DeleteCommand<V1WorkflowInstance, string>(id), cancellationToken), (int)HttpStatusCode.Created);
        }

    }

}