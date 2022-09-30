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
using Synapse.Integration.Commands.AuthenticationDefinitionCollections;
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Commands.EventDefinitionCollections;
using Synapse.Integration.Commands.FunctionDefinitionCollections;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;

namespace Synapse.Apis.Management.Grpc
{

    /// <summary>
    /// Represents a GRPC-based client for the <see cref="ISynapseManagementApi"/>
    /// </summary>
    public class SynapseGrpcManagementApiClient
        : ISynapseManagementApi
    {

        /// <summary>
        /// Initializes a new <see cref="ISynapseGrpcManagementApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="adapter">The service used to interact with the GRPC port of the Synapse API</param>
        public SynapseGrpcManagementApiClient(ILogger<SynapseGrpcManagementApiClient> logger, ISynapseGrpcManagementApi adapter)
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
        protected ISynapseGrpcManagementApi Adapter { get; }

        #region Application

        /// <inheritdoc/>
        public virtual async Task<V1ApplicationInfo> GetApplicationInfoAsync(CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetApplicationInfoAsync(cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        #endregion

        #region Workflows

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> CreateWorkflowAsync(V1CreateWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateWorkflowAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> UploadWorkflowAsync(V1UploadWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.UploadWorkflowAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Workflow>> GetWorkflowsAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowsAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> ArchiveWorkflowAsync(string id, string? version = null, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.ArchiveWorkflowAsync(id, version, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return new MemoryStream(result.Data!);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
        {
            await this.Adapter.DeleteWorkflowAsync(id, cancellationToken);
        }

        #endregion region

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateWorkflowInstanceAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.StartWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowInstanceByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowInstancesAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowInstancesAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SuspendWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.SuspendWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        /// <inheritdoc/>
        public virtual async Task ResumeWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.ResumeWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        /// <inheritdoc/>
        public virtual async Task CancelWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CancelWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> ArchiveWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.ArchiveWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return new MemoryStream(result.Data!);
        }

        /// <inheritdoc/>
        public virtual async Task<string> GetWorkflowInstanceLogsAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetWorkflowInstanceLogsAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.DeleteWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        #endregion

        #region Correlations

        /// <inheritdoc/>
        public virtual async Task<V1Correlation> CreateCorrelationAsync(V1CreateCorrelationCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateCorrelationAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Correlation>> GetCorrelationsAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetCorrelationsAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Correlation>> GetCorrelationsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetCorrelationsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Correlation> GetCorrelationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetCorrelationByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteCorrelationAsync(string id, CancellationToken cancellationToken = default)
        {
            await this.Adapter.DeleteCorrelationAsync(id, cancellationToken);
        }


        #endregion region

        #region AuthenticationDefinitionCollections

        /// <inheritdoc/>
        public virtual async Task<V1AuthenticationDefinitionCollection> CreateAuthenticationDefinitionCollectionAsync(V1CreateAuthenticationDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateAuthenticationDefinitionCollectionAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1AuthenticationDefinitionCollection> GetAuthenticationDefinitionCollectionByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetAuthenticationDefinitionCollectionByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1AuthenticationDefinitionCollection>> GetAuthenticationDefinitionCollectionsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetAuthenticationDefinitionCollectionsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1AuthenticationDefinitionCollection>> GetAuthenticationDefinitionCollectionsAsync(string? query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetAuthenticationDefinitionCollectionsAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAuthenticationDefinitionCollectionAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.DeleteWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        #endregion

        #region EventDefinitionCollections

        /// <inheritdoc/>
        public virtual async Task<V1EventDefinitionCollection> CreateEventDefinitionCollectionAsync(V1CreateEventDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateEventDefinitionCollectionAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1EventDefinitionCollection> GetEventDefinitionCollectionByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetEventDefinitionCollectionByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1EventDefinitionCollection>> GetEventDefinitionCollectionsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetEventDefinitionCollectionsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1EventDefinitionCollection>> GetEventDefinitionCollectionsAsync(string? query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetEventDefinitionCollectionsAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteEventDefinitionCollectionAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.DeleteWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        #endregion

        #region FunctionDefinitionCollections

        /// <inheritdoc/>
        public virtual async Task<V1FunctionDefinitionCollection> CreateFunctionDefinitionCollectionAsync(V1CreateFunctionDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.CreateFunctionDefinitionCollectionAsync(command, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<V1FunctionDefinitionCollection> GetFunctionDefinitionCollectionByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetFunctionDefinitionCollectionByIdAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1FunctionDefinitionCollection>> GetFunctionDefinitionCollectionsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetFunctionDefinitionCollectionsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1FunctionDefinitionCollection>> GetFunctionDefinitionCollectionsAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.GetFunctionDefinitionCollectionsAsync(query, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteFunctionDefinitionCollectionAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.Adapter.DeleteWorkflowInstanceAsync(id, cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
        }

        #endregion

        #region OperationalReports

        /// <inheritdoc/>
        public virtual async Task<V1OperationalReport> GetOperationalReportAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            if (!date.HasValue)
                date = DateTime.Now;
            var result = await this.Adapter.GetOperationalReportAsync(new(date.Value), cancellationToken);
            if (!result.Succeeded)
                throw new SynapseApiException(result);
            return result.Data!;
        }

        #endregion

    }

}
