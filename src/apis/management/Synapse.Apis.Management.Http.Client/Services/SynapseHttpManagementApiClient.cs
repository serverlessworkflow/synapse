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
using Synapse.Integration.Commands.AuthenticationDefinitionCollections;
using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Commands.EventDefinitionCollections;
using Synapse.Integration.Commands.FunctionDefinitionCollections;
using Synapse.Integration.Commands.Schedules;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using System.Net.Mime;
using System.Text;

namespace Synapse.Apis.Management.Http;

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

    /// <summary>
    /// Creates a new <see cref="HttpRequestMessage"/>
    /// </summary>
    /// <param name="method">The <see cref="HttpMethod"/> of the <see cref="HttpRequestMessage"/> to create</param>
    /// <param name="requestUri">The request <see cref="Uri"/> of the <see cref="HttpRequestMessage"/> to create</param>
    /// <param name="content">The <see cref="HttpContent"/> of the <see cref="HttpRequestMessage"/> to create</param>
    /// <returns>A new <see cref="HttpRequestMessage"/></returns>
    protected virtual HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, requestUri) { Content = content };
        return request;
    }

    #region Application

    /// <inheritdoc/>
    public virtual async Task<V1ApplicationInfo> GetApplicationInfoAsync(CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/application/info";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while retrieving information about the running Synapse application: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1ApplicationInfo>(json, cancellationToken);
    }

    #endregion

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
    public virtual async Task<V1Workflow> UploadWorkflowAsync(V1UploadWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/workflows/upload";
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(command.DefinitionFile.OpenReadStream()), nameof(V1UploadWorkflowCommand.DefinitionFile), command.DefinitionFile.FileName);
        foreach (var resourceFile in command.ResourceFiles)
        {
            content.Add(new StreamContent(resourceFile.OpenReadStream()), nameof(V1UploadWorkflowCommand.ResourceFiles), resourceFile.FileName);
        }
        using var request = this.CreateRequest(HttpMethod.Post, requestUri);
        request.Content = content;
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
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
    public virtual async Task<List<V1Workflow>> SearchWorkflowsAsync(string term, string? query = null, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/odata/v1workflows";
        if (!string.IsNullOrWhiteSpace(query))
            requestUri += $"?$search={term}{(string.IsNullOrWhiteSpace(query) ? string.Empty : $"&{query}")}";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying workflows: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<List<V1Workflow>>(json, cancellationToken);
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
    public virtual async Task<Stream> ArchiveWorkflowAsync(string id, string? version = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/workflows/{id}/archive";
        return await this.HttpClient.GetStreamAsync(requestUri, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/workflows/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the workflow with the specified id '{id}': {details}", id, json);
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

    /// <inheritdoc/>
    public virtual async Task SuspendWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/workflow-instances/{id}/suspend";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while suspending the execution of the workflow with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task ResumeWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/workflow-instances/{id}/resume";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while resuming the execution of the workflow with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task CancelWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/workflow-instances/{id}/cancel";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while cancelling the execution of the workflow instance with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task<string> GetWorkflowInstanceLogsAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/workflow-instances/{id}/logs";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying the log of the workflow instance with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
        return json;
    }

    /// <inheritdoc/>
    public virtual async Task<Stream> ArchiveWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/workflow-instances/{id}/logs";
        return await this.HttpClient.GetStreamAsync(requestUri, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/workflow-instances/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying deleting the workflow instance with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
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
    public virtual async Task DeleteCorrelationContextAsync(string correlationId, string contextId, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/correlations/byid/{correlationId}/contexts/{contextId}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the context with id '{contextId}', owned by the correlation with id '{correlationId}': {details}", contextId, correlationId, json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task DeleteCorrelationContextEventAsync(string correlationId, string contextId, string eventId, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/correlations/byid/{correlationId}/contexts/{contextId}/events/{eventId}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the event with id '{eventId}', owned by the correlation context with id '{contextId}': {details}", eventId, contextId, json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task DeleteCorrelationAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/correlations/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the correlation with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    #endregion region

    #region AuthenticationDefinitionCollections

    /// <inheritdoc/>
    public virtual async Task<V1AuthenticationDefinitionCollection> CreateAuthenticationDefinitionCollectionAsync(V1CreateAuthenticationDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/resources/collections/authentications";
        using var request = this.CreateRequest(HttpMethod.Post, requestUri);
        var json = await this.Serializer.SerializeAsync(command, cancellationToken);
        request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while creating a new authentication definition collection: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1AuthenticationDefinitionCollection>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<V1AuthenticationDefinitionCollection> GetAuthenticationDefinitionCollectionByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/resources/collections/authentications/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying authentication definition collections: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1AuthenticationDefinitionCollection>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<V1AuthenticationDefinitionCollection>> GetAuthenticationDefinitionCollectionsAsync(string? query, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/resources/collections/authentications";
        if (!string.IsNullOrWhiteSpace(query))
            requestUri += $"?{query}";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying authentication definition collections: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<List<V1AuthenticationDefinitionCollection>>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAuthenticationDefinitionCollectionAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/resources/collections/authentications/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the authentication definition collection with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region EventDefinitionCollections

    /// <inheritdoc/>
    public virtual async Task<V1EventDefinitionCollection> CreateEventDefinitionCollectionAsync(V1CreateEventDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/resources/collections/events";
        using var request = this.CreateRequest(HttpMethod.Post, requestUri);
        var json = await this.Serializer.SerializeAsync(command, cancellationToken);
        request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while creating a new e definition collection: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1EventDefinitionCollection>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<V1EventDefinitionCollection> GetEventDefinitionCollectionByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/resources/collections/events/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying e definition collections: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1EventDefinitionCollection>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<V1EventDefinitionCollection>> GetEventDefinitionCollectionsAsync(string? query, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/resources/collections/events";
        if (!string.IsNullOrWhiteSpace(query))
            requestUri += $"?{query}";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying e definition collections: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<List<V1EventDefinitionCollection>>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteEventDefinitionCollectionAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/resources/collections/events/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the e definition collection with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region FunctionDefinitionCollections

    /// <inheritdoc/>
    public virtual async Task<V1FunctionDefinitionCollection> CreateFunctionDefinitionCollectionAsync(V1CreateFunctionDefinitionCollectionCommand command, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/resources/collections/functions";
        using var request = this.CreateRequest(HttpMethod.Post, requestUri);
        var json = await this.Serializer.SerializeAsync(command, cancellationToken);
        request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while creating a new function definition collection: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1FunctionDefinitionCollection>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<V1FunctionDefinitionCollection> GetFunctionDefinitionCollectionByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/resources/collections/functions/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying function definition collections: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1FunctionDefinitionCollection>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<V1FunctionDefinitionCollection>> GetFunctionDefinitionCollectionsAsync(string? query, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/resources/collections/functions";
        if (!string.IsNullOrWhiteSpace(query))
            requestUri += $"?{query}";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying function definition collections: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<List<V1FunctionDefinitionCollection>>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteFunctionDefinitionCollectionAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/resources/collections/functions/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the function definition collection with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region OperationalReports

    /// <inheritdoc/>
    public virtual async Task<V1OperationalReport> GetOperationalReportAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        if (date == null)
            date = DateTime.Now;
        using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/reports/operations?date={date}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying operation reports: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1OperationalReport>(json, cancellationToken);
    }

    #endregion

    #region Schedules

    /// <inheritdoc/>
    public virtual async Task<V1Schedule> CreateScheduleAsync(V1CreateScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/schedules";
        using var request = this.CreateRequest(HttpMethod.Post, requestUri);
        var json = await this.Serializer.SerializeAsync(command, cancellationToken);
        request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while creating a new schedule: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1Schedule>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<V1Schedule> GetScheduleByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Get, $"/api/v1/schedules/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying schedules: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<V1Schedule>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<V1Schedule>> GetSchedulesAsync(string query, CancellationToken cancellationToken = default)
    {
        var requestUri = "/api/v1/schedules";
        if (!string.IsNullOrWhiteSpace(query))
            requestUri += $"?{query}";
        using var request = this.CreateRequest(HttpMethod.Get, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while querying schedules: {details}", json);
        response.EnsureSuccessStatusCode();
        return await this.Serializer.DeserializeAsync<List<V1Schedule>>(json, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<V1Schedule>> GetSchedulesAsync(CancellationToken cancellationToken = default) => await this.GetSchedulesAsync(null!, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task TriggerScheduleAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/schedules/{id}/trigger";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while triggering a new schedule occurence: {details}", json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task SuspendScheduleAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/schedules/{id}/suspend";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while suspending a schedule: {details}", json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task ResumeScheduleAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/schedules/{id}/resume";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while resuming a schedule: {details}", json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task RetireScheduleAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/schedules/{id}/retire";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while retiring a schedule: {details}", json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task MakeScheduleObsoleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/v1/schedules/{id}/obsolete";
        using var request = this.CreateRequest(HttpMethod.Put, requestUri);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while making a schedule obsolete: {details}", json);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual async Task DeleteScheduleAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = this.CreateRequest(HttpMethod.Delete, $"/api/v1/schedules/{id}");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
            this.Logger.LogError("An error occured while deleting the schedule with the specified id '{id}': {details}", id, json);
        response.EnsureSuccessStatusCode();
    }

    #endregion

}

