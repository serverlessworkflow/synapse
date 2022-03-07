using Synapse.Application.Commands.Workflows;
using Synapse.Integration.Commands.Generic;
using Synapse.Integration.Commands.Workflows;

namespace Synapse.Ports.HttpRest.Controllers
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
        [ProducesResponseType(typeof(V1WorkflowDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Create([FromBody] V1CreateWorkflowCommandDto command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1CreateWorkflowCommand>(command), cancellationToken), (int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Gets the workflow with the specified id.
        /// </summary>
        /// <param name="id">The id of the workflow to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}"), EnableQuery]
        [ProducesResponseType(typeof(V1WorkflowDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1WorkflowDto, string>(id), cancellationToken));
        }

        /// <summary>
        /// Gets the workflow instance with the specified id.
        /// </summary>
        /// <param name="id">The id of the workflow of the instance to get</param>
        /// <param name="instanceKey">The key of the specified workflow's instance to find</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet("{id}/{instanceKey}"), EnableQuery]
        [ProducesResponseType(typeof(V1WorkflowInstanceDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetInstanceByKey(string id, string instanceKey, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1WorkflowInstanceDto, string>($"{id}-{instanceKey}"), cancellationToken));
        }

        /// <summary>
        /// Queries workflows
        /// <para>This endpoint supports ODATA.</para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpGet, EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<V1WorkflowDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1ListQuery<V1WorkflowDto>(), cancellationToken));
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
        public async Task<IActionResult> Patch([FromBody] V1PatchCommandDto command, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1PatchCommand<V1Workflow, string>>(command), cancellationToken));
        }

        /// <summary>
        /// Deletes an existing workflow
        /// </summary>
        /// <param name="id">The id of the workflow to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IActionResult"/></returns>
        [HttpDelete("byid/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            return this.Process(await this.Mediator.ExecuteAsync(new V1DeleteCommand<V1Workflow, string>(id), cancellationToken), (int)HttpStatusCode.Created);
        }

    }

}
