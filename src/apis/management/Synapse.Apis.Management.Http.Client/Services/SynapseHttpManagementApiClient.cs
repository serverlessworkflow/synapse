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
using Neuroglia.Serialization;
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using System.Net.Mime;
using System.Text;

namespace Synapse.Apis.Management.Http
{

    /// <summary>
    /// Represents the service used to interact with the Synapse HTTP REST API
    /// </summary>
    public class SynapseHttpManagementApiClient
        : ISynapseManagementApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseHttpManagementApiClient"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="serializer">The service used to serialize/deserialize to/from JSON</param>
        public SynapseHttpManagementApiClient(ILogger<SynapseHttpManagementApiClient> logger, IHttpClientFactory httpClientFactory, IJsonSerializer serializer)
        {
            this.Logger = logger;
            this.HttpClient = httpClientFactory.CreateClient(this.GetType().Name);
            this.Serializer = serializer;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used to request the Synapse HTTP REST API
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer Serializer { get; }

        protected virtual HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, requestUri) { Content = content };
            return request;
        }

        #region Workflows

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> CreateWorkflowAsync(V1CreateWorkflowCommand command, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/workflows";
            using var request = this.CreateRequest(HttpMethod.Post, requestUri);
            var json = await this.Serializer.SerializeAsync(command, cancellationToken);
            request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while creating a new workflow: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1Workflow>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Workflow>> GetWorkflowsAsync(string? query, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/workflows";
            if (!string.IsNullOrWhiteSpace(query))
                requestUri += $"?{query}";
            using var request = this.CreateRequest(HttpMethod.Get, requestUri);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<List<V1Workflow>>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Workflow> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/workflows/{id}");
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1Workflow>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
        {
            using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/workflows/{id}");
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
        }

        #endregion region

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommand command, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/workflow-instances";
            using var request = this.CreateRequest(HttpMethod.Post, requestUri);
            var json = await this.Serializer.SerializeAsync(command, cancellationToken);
            request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while creating a new workflow instance: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1WorkflowInstance>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
        {
            var requestUri = $"/api/v1/workflow-instances/byid/{id}/start";
            using var request = this.CreateRequest(HttpMethod.Put, requestUri);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while starting the workflow instance with id '{workflowInstanceId}': {details}", id, json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1WorkflowInstance>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstance> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/workflow-instances/{id}");
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflow instances: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1WorkflowInstance>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(string? query, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/workflow-instances";
            if (!string.IsNullOrWhiteSpace(query))
                requestUri += $"?{query}";
            using var request = this.CreateRequest(HttpMethod.Get, requestUri);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<List<V1WorkflowInstance>>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowInstancesAsync(null, cancellationToken);
        }

        #endregion

        #region Correlations

        /// <inheritdoc/>
        public virtual async Task<V1Correlation> CreateCorrelationAsync(V1CreateCorrelationCommand command, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/correlations";
            using var request = this.CreateRequest(HttpMethod.Post, requestUri);
            var json = await this.Serializer.SerializeAsync(command, cancellationToken);
            request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while creating a new correlation: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1Correlation>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Correlation>> GetCorrelationsAsync(string? query, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/correlations";
            if (!string.IsNullOrWhiteSpace(query))
                requestUri += $"?{query}";
            using var request = this.CreateRequest(HttpMethod.Get, requestUri);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying correlations: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<List<V1Correlation>>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1Correlation>> GetCorrelationsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetCorrelationsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1Correlation> GetCorrelationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/correlations/{id}");
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying correlations: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1Correlation>(json, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteCorrelationAsync(string id, CancellationToken cancellationToken = default)
        {
            using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/correlations/{id}");
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying correlations: {details}", json);
            response.EnsureSuccessStatusCode();
        }

        #endregion region

    }

}
