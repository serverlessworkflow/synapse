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
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using Synapse.Integration.Services;

namespace Synapse.Ports.Grpc.Services
{

    /// <summary>
    /// Represents a GRPC-based client for the <see cref="ISynapseApi"/>
    /// </summary>
    public class SynapseGrpcApiClient
        : ISynapseApi
    {

        /// <summary>
        /// Initializes a new <see cref="ISynapseGrpcApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="adapter">The service used to interact with the GRPC port of the Synapse API</param>
        public SynapseGrpcApiClient(ILogger<SynapseGrpcApiClient> logger, ISynapseGrpcApi adapter)
        {
            this.Logger = logger;
            this.Adapter = adapter;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to interact with the GRPC port of the Synapse API
        /// </summary>
        protected ISynapseGrpcApi Adapter { get; }

        #region Workflows

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowDto> CreateWorkflowAsync(V1CreateWorkflowCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateWorkflowAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowDto>> GetWorkflowsAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowsAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowDto>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowDto> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
        {
            await this.Adapter.DeleteWorkflowAsync(id, cancellationToken);
        }

        #endregion region

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommandDto command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateWorkflowInstanceAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.StartWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowInstanceByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstanceDto>> GetWorkflowInstancesAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowInstancesAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstanceDto>> GetWorkflowInstancesAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowInstancesAsync(null!, cancellationToken);
        }


        #endregion

    }

}
