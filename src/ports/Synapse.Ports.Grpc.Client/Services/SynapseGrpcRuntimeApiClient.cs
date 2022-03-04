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
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;
using Synapse.Integration.Services;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Represents a GRPC-based client for the <see cref="ISynapseApi"/>
    /// </summary>
    public class SynapseGrpcRuntimeApiClient
        : ISynapseRuntimeApi
    {

        /// <summary>
        /// Initializes a new <see cref="ISynapseGrpcApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="grpcRuntimeApi">The service used to interact with the GRPC port of the Synapse Runtime API</param>
        public SynapseGrpcRuntimeApiClient(ILogger<SynapseGrpcRuntimeApiClient> logger, ISynapseGrpcRuntimeApi grpcRuntimeApi)
        {
            this.Logger = logger;
            this.GrpcRuntimeApi = grpcRuntimeApi;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to interact with the GRPC port of the Synapse Runtime API
        /// </summary>
        protected ISynapseGrpcRuntimeApi GrpcRuntimeApi { get; }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivityDto>> GetActivitiesAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.GetActivitiesAsync(new(workflowInstanceId, true), cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivityDto>> GetOperativeActivitiesAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.GetActivitiesAsync(new(workflowInstanceId), cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivityDto>> GetActivitiesAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.GetActivitiesAsync(new(workflowInstanceId, true, activityId), cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivityDto>> GetOperativeActivitiesAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.GetActivitiesAsync(new(workflowInstanceId, false, activityId), cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> StartAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.StartAsync(workflowInstanceId, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> CreateActivityAsync(V1CreateWorkflowActivityCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.CreateActivityAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> StartActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.StartActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> SuspendActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.SuspendActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> SkipActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.SkipActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> FaultActivityAsync(V1FaultWorkflowActivityCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.FaultActivityAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> CancelActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.CancelActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivityDto> SetActivityOutputAsync(V1SetWorkflowActivityOutputCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.SetActivityOutputAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> FaultAsync(V1FaultWorkflowInstanceCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.FaultAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.CancelAsync(workflowInstanceId, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> SetOutputAsync(V1SetWorkflowInstanceOutputCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.GrpcRuntimeApi.SetOutputAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

    }

}
