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
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Application.Commands.Generic;
using Synapse.Application.Commands.WorkflowInstances;
using Synapse.Application.Commands.Workflows;
using Synapse.Application.Queries.Generic;
using Synapse.Application.Services;
using Synapse.Domain.Models;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using Synapse.Integration.Services;
using Synapse.Ports.Grpc.Models;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcApi"/>, which acts as a GRPC adapter of the <see cref="ISynapseApi"/>
    /// </summary>
    public class SynapseGrpcApi
        : ISynapseGrpcApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseGrpcApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="queryOptionsParser">The service used to parse <see cref="ODataQueryOptions"/></param>
        public SynapseGrpcApi(ILogger<SynapseGrpcApi> logger, IMediator mediator, IMapper mapper, IODataQueryOptionsParser queryOptionsParser)
        {
            this.Logger = logger;
            this.Mediator = mediator;
            this.Mapper = mapper;
            this.QueryOptionsParser = queryOptionsParser;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

        /// <summary>
        /// Gets the service used to parse <see cref="ODataQueryOptions"/>
        /// </summary>
        protected IODataQueryOptionsParser QueryOptionsParser { get; }

        #region Workflows

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowDto>> CreateWorkflowAsync(V1CreateWorkflowCommandDto command, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1CreateWorkflowCommand>(command), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowDto>> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1WorkflowDto, string>(id), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<List<V1WorkflowDto>>> GetWorkflowsAsync(string? query = null, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FilterQuery<V1WorkflowDto>(this.QueryOptionsParser.Parse<V1WorkflowDto>(query)), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult> DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1DeleteCommand<V1Workflow, string>>(id), cancellationToken));
        }

        #endregion

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommandDto command, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1CreateWorkflowInstanceCommand>(command), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1StartWorkflowInstanceCommand(id), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1WorkflowInstanceDto, string>(id), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<List<V1WorkflowInstanceDto>>> GetWorkflowInstancesAsync(string? query = null, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FilterQuery<V1WorkflowInstanceDto>(this.QueryOptionsParser.Parse<V1WorkflowInstanceDto>(query)), cancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult> DeleteWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1DeleteCommand<V1WorkflowInstance, string>>(id), cancellationToken));
        }


        #endregion
    }

}
