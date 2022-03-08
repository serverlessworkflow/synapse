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
using Synapse.Integration.Commands.WorkflowActivities;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Models;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Represents the default GRPC-based implementation of the <see cref="ISynapseRuntimeApi"/> interface
    /// </summary>
    public class SynapseGrpcRuntimeApiClient
        : ISynapseRuntimeApi
    {

        /// <summary>
        /// Initializes a new <see cref="ISynapseGrpcApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="runtimeApi">The service used to interact with the GRPC port of the Synapse Runtime API</param>
        public SynapseGrpcRuntimeApiClient(ILogger<SynapseGrpcRuntimeApiClient> logger, ISynapseGrpcRuntimeApi runtimeApi)
        {
            this.Logger = logger;
            this.RuntimeApi = runtimeApi;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to interact with the GRPC port of the Synapse Runtime API
        /// </summary>
        protected ISynapseGrpcRuntimeApi RuntimeApi { get; }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<RuntimeSignal> Connect(string workflowInstanceId)
        {
            return this.RuntimeApi.Connect(workflowInstanceId);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetActivitiesAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.GetActivitiesAsync(new(workflowInstanceId, true), cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetOperativeActivitiesAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.GetActivitiesAsync(new(workflowInstanceId), cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetActivitiesAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.GetActivitiesAsync(new(workflowInstanceId, true, activityId), cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowActivity>> GetOperativeActivitiesAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.GetActivitiesAsync(new(workflowInstanceId, false, activityId), cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data ?? (new());
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> StartAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.StartAsync(workflowInstanceId, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CreateActivityAsync(V1CreateWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.CreateActivityAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> StartActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.StartActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SuspendActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.SuspendActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SkipActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.SkipActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> FaultActivityAsync(V1FaultWorkflowActivityCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.FaultActivityAsync(command, cancellationToken);
            if (!result.Succeeded)
                   throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> CancelActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.CancelActivityAsync(activityId, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowActivity> SetActivityOutputAsync(V1SetWorkflowActivityOutputCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.SetActivityOutputAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> FaultAsync(V1FaultWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.FaultAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.CancelAsync(workflowInstanceId, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> SetOutputAsync(V1SetWorkflowInstanceOutputCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.RuntimeApi.SetOutputAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new OperationResultException(new OperationResult(result.Code, result.Errors?.Select(e => new Neuroglia.Error(e.Code, e.Message))?.ToArray()));
            return result.Data!;
        }

    }

}
