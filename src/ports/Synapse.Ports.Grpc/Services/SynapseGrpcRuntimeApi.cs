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
using Grpc.Core;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using ProtoBuf.Grpc;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Application.Commands.WorkflowActivities;
using Synapse.Application.Commands.WorkflowInstances;
using Synapse.Application.Queries.WorkflowActivities;
using Synapse.Application.Services;
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;
using Synapse.Integration.Services;
using Synapse.Ports.Grpc.Models;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcApi"/>, which acts as a GRPC adapter of the <see cref="ISynapseApi"/>
    /// </summary>
    public class SynapseGrpcRuntimeApi
        : ISynapseGrpcRuntimeApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseGrpcRuntimeApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="queryOptionsParser">The service used to parse <see cref="ODataQueryOptions"/></param>
        public SynapseGrpcRuntimeApi(ILogger<SynapseGrpcApi> logger, IMediator mediator, IMapper mapper, IODataQueryOptionsParser queryOptionsParser)
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

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowActivityDto>> CreateActivityAsync(V1CreateWorkflowActivityCommandDto command, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1CreateWorkflowActivityCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<List<V1WorkflowActivityDto>>> GetActivitiesAsync(string workflowInstanceId, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1GetWorkflowActivitiesQuery(workflowInstanceId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowActivityDto>> StartActivityAsync(string activityId, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1StartWorkflowActivityCommand(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowActivityDto>> SuspendActivityAsync(string activityId, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1SuspendWorkflowActivityCommand(activityId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowActivityDto>> FaultActivityAsync(V1FaultWorkflowActivityCommandDto command, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1FaultWorkflowActivityCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowActivityDto>> CancelActivityAsync(string activityId, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1CancelWorkflowActivityCommand(activityId), context.CancellationToken));
        }   

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowActivityDto>> SetActivityOutputAsync(V1SetWorkflowActivityOutputCommandDto command, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1SetWorkflowActivityOutputCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> StartAsync(string workflowInstanceId, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1SetWorkflowInstanceStartedCommand(workflowInstanceId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> FaultAsync(V1FaultWorkflowInstanceCommandDto command, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1FaultWorkflowInstanceCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> CancelAsync(string workflowInstanceId, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1CancelWorkflowInstanceCommand(workflowInstanceId), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<V1GrpcApiResult<V1WorkflowInstanceDto>> SetOutputAsync(V1SetWorkflowInstanceOutputCommandDto command, CallContext context = default)
        {
            return V1GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<V1SetWorkflowInstanceOutputCommand>(command), context.CancellationToken));
        }

    }

}
