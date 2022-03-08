﻿/*
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
using Microsoft.Extensions.Logging;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using ProtoBuf.Grpc;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Apis.Management.Grpc.Models;
using Synapse.Application.Commands.Generic;
using Synapse.Application.Queries.Generic;
using Synapse.Application.Services;
using Synapse.Integration.Models;

namespace Synapse.Apis.Management.Grpc
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcManagementApi"/>, which acts as a GRPC adapter of the <see cref="ISynapseApi"/>
    /// </summary>
    public class SynapseGrpcManagementApi
        : ISynapseGrpcManagementApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseGrpcManagementApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="queryOptionsParser">The service used to parse <see cref="ODataQueryOptions"/></param>
        public SynapseGrpcManagementApi(ILogger<SynapseGrpcManagementApi> logger, IMediator mediator, IMapper mapper, IODataQueryOptionsParser queryOptionsParser)
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
        public virtual async Task<GrpcApiResult<V1Workflow>> CreateWorkflowAsync(Integration.Commands.Workflows.V1CreateWorkflowCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Workflows.V1CreateWorkflowCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1Workflow>> GetWorkflowByIdAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1Workflow, string>(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<List<V1Workflow>>> GetWorkflowsAsync(string? query = null, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FilterQuery<V1Workflow>(this.QueryOptionsParser.Parse<V1Workflow>(query)), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> DeleteWorkflowAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1DeleteCommand<V1Workflow, string>>(id), context.CancellationToken));
        }

        #endregion

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> CreateWorkflowInstanceAsync(Integration.Commands.WorkflowInstances.V1CreateWorkflowInstanceCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.WorkflowInstances.V1CreateWorkflowInstanceCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> StartWorkflowInstanceAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1StartWorkflowInstanceCommand(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1WorkflowInstance>> GetWorkflowInstanceByIdAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1WorkflowInstance, string>(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<List<V1WorkflowInstance>>> GetWorkflowInstancesAsync(string? query = null, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FilterQuery<V1WorkflowInstance>(this.QueryOptionsParser.Parse<V1WorkflowInstance>(query)), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> DeleteWorkflowInstanceAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1DeleteCommand<V1WorkflowInstance, string>>(id), context.CancellationToken));
        }


        #endregion
    }

}