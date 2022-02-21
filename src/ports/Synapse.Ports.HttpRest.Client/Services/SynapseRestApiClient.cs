using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Neuroglia.Serialization;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using Synapse.Integration.Services;

namespace Synapse.Ports.HttpRest.Client.Services
{

    /// <summary>
    /// Represents the service used to interact with the Synapse HTTP REST API
    /// </summary>
    public class SynapseRestApiClient
        : ISynapseApi
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseRestApiClient"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="serializer">The service used to serialize/deserialize to/from JSON</param>
        public SynapseRestApiClient(ILogger<SynapseRestApiClient> logger, IHttpClientFactory httpClientFactory, IJsonSerializer serializer)
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
        public virtual async Task<V1WorkflowDto> CreateWorkflowAsync(V1CreateWorkflowCommandDto command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowDto>> GetWorkflowsAsync(string? query, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/workflows";
            if (!string.IsNullOrWhiteSpace(query))
                requestUri += $"?{query}";
            using var request = this.CreateRequest(HttpMethod.Get, requestUri);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync()!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<List<V1WorkflowDto>>(json);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowDto>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowsAsync(null!, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowDto> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/workflows/{id}");
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync()!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<V1WorkflowDto>(json);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion region

        #region WorkflowInstances

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommandDto command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<V1WorkflowInstanceDto> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstanceDto>> GetWorkflowInstancesAsync(string? query, CancellationToken cancellationToken = default)
        {
            var requestUri = "/api/v1/workflows";
            if (!string.IsNullOrWhiteSpace(query))
                requestUri += $"?{query}";
            using var request = this.CreateRequest(HttpMethod.Get, requestUri);
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync()!;
            if (!response.IsSuccessStatusCode)
                this.Logger.LogError("An error occured while querying workflows: {details}", json);
            response.EnsureSuccessStatusCode();
            return await this.Serializer.DeserializeAsync<List<V1WorkflowInstanceDto>>(json);
        }

        /// <inheritdoc/>
        public virtual async Task<List<V1WorkflowInstanceDto>> GetWorkflowInstancesAsync(CancellationToken cancellationToken = default)
        {
            return await this.GetWorkflowInstancesAsync(null, cancellationToken);
        }

        #endregion

    }

}
