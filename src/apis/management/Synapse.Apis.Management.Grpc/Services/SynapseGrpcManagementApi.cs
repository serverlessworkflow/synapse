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

using Microsoft.Extensions.Logging;
using Neuroglia;
using Neuroglia.Mapping;
using Neuroglia.Mediation;
using ProtoBuf.Grpc;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Apis.Management.Grpc.Models;
using Synapse.Application.Commands.Generic;
using Synapse.Application.Queries.Generic;
using Synapse.Application.Services;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;

namespace Synapse.Apis.Management.Grpc
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcManagementApi"/>, which acts as a GRPC adapter of the Synapse API
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
        public virtual async Task<GrpcApiResult<V1Workflow>> CreateWorkflowAsync(V1CreateWorkflowCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Workflows.V1CreateWorkflowCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual Task<GrpcApiResult<V1Workflow>> UploadWorkflowAsync(V1UploadWorkflowCommand command, CallContext context = default)
        {
            throw new NotSupportedException();
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
        public virtual async Task<GrpcApiResult<byte[]>> ArchiveWorkflowAsync(string id, string? version = null, CallContext context = default)
        {
            var result = await this.Mediator.ExecuteAsync(new Application.Commands.Workflows.V1ArchiveWorkflowCommand(id, version), context.CancellationToken);
            OperationResult<byte[]> toReturn;
            if (result.Succeeded)
                toReturn = new(((MemoryStream)result.Data!).ToArray());
            else
                toReturn = new(result.Code, result.Errors?.ToArray());
            return GrpcApiResult.CreateFor(toReturn);
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> DeleteWorkflowAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.Workflows.V1DeleteWorkflowCommand(id), context.CancellationToken));
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
        public virtual async Task<GrpcApiResult> SuspendWorkflowInstanceAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1SuspendWorkflowInstanceCommand(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> ResumeWorkflowInstanceAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1ResumeWorkflowInstanceCommand(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> CancelWorkflowInstanceAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1CancelWorkflowInstanceCommand(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<byte[]>> ArchiveWorkflowInstanceAsync(string id, CallContext context = default)
        {
            var result = await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1ArchiveWorkflowInstanceCommand(id), context.CancellationToken);
            OperationResult<byte[]> toReturn;
            if (result.Succeeded)
                toReturn = new(((MemoryStream)result.Data!).ToArray());
            else
                toReturn = new(result.Code, result.Errors?.ToArray());
            return GrpcApiResult.CreateFor(toReturn);
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> DeleteWorkflowInstanceAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Commands.WorkflowInstances.V1DeleteWorkflowInstanceCommand(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<string>> GetWorkflowInstanceLogsAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new Application.Queries.WorkflowInstances.V1GetWorkflowInstanceLogsQuery(id), context.CancellationToken));
        }

        #endregion

        #region Correlations

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1Correlation>> CreateCorrelationAsync(Integration.Commands.Correlations.V1CreateCorrelationCommand command, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(this.Mapper.Map<Application.Commands.Correlations.V1CreateCorrelationCommand>(command), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1Correlation>> GetCorrelationByIdAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1Correlation, string>(id), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<List<V1Correlation>>> GetCorrelationsAsync(string? query = null, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FilterQuery<V1Correlation>(this.QueryOptionsParser.Parse<V1Correlation>(query)), context.CancellationToken));
        }

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult> DeleteCorrelationAsync(string id, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1DeleteCommand<V1Correlation, string>(id), context.CancellationToken));
        }


        #endregion

        #region OperationalReports

        /// <inheritdoc/>
        public virtual async Task<GrpcApiResult<V1OperationalReport>> GetOperationalReportAsync(GrpcApiRequest<DateTime> request, CallContext context = default)
        {
            return GrpcApiResult.CreateFor(await this.Mediator.ExecuteAsync(new V1FindByIdQuery<V1OperationalReport, string>(V1OperationalReport.GetIdFor(request.Data)), context.CancellationToken));
        }

        #endregion

    }

}
