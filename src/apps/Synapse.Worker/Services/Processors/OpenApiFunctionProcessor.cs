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

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapse.Integration.Events.WorkflowActivities;
using System.Collections;
using System.Dynamic;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process Open API <see cref="FunctionDefinition"/>s
    /// </summary>
    public class OpenApiFunctionProcessor
        : FunctionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="OpenApiFunctionProcessor"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public OpenApiFunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, 
            IHttpClientFactory httpClientFactory, ISerializerProvider serializerProvider, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, 
            ActionDefinition action, FunctionDefinition function) 
            : base(serviceProvider, loggerFactory, context, activityProcessorFactory, options, activity, action, function)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.SerializerProvider = serializerProvider;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used by the <see cref="FunctionProcessor"/> to execute the function
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the service used to provide <see cref="ISerializer"/>s
        /// </summary>
        protected ISerializerProvider SerializerProvider { get; }

        /// <summary>
        /// Gets the <see cref="OpenApiDocument"/> that defines the operation to invoke
        /// </summary>
        protected OpenApiDocument Document { get; private set; } = null!;

        /// <summary>
        /// Gets the id of the <see cref="OpenApiOperation"/> to invoke, as defined by the <see cref="FunctionDefinition.Operation"/> property
        /// </summary>
        protected OpenApiOperation Operation { get; private set; } = null!;

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpMethod"/> to use to invoke the <see cref="Operation"/>
        /// </summary>
        protected HttpMethod HttpMethod { get; private set; } = null!;

        /// <summary>
        /// Gets/sets a <see cref="List{T}"/> containing the addresses of the available remote servers
        /// </summary>
        protected List<string> Servers { get; private set; } = null!;

        /// <summary>
        /// Gets/sets the uri's path segment of the function to execute
        /// </summary>
        protected string Path { get; private set; } = null!;

        /// <summary>
        /// Gets/sets an <see cref="IDictionary{TKey, TValue}"/> that represents the parameters to pass to the function to execute
        /// </summary>
        protected IDictionary<string, object> Parameters { get; private set; } = null!;

        /// <summary>
        /// Gets/sets the uri's query string segment of the function to execute
        /// </summary>
        protected string QueryString { get; private set; } = null!;

        /// <summary>
        /// Gets/sets the body of the <see cref="HttpRequestMessage"/> sent to invoke the function
        /// </summary>
        protected object? Body { get; private set; }

        /// <summary>
        /// Gets/sets a <see cref="Dictionary{TKey, TValue}"/> containing the headers of the <see cref="HttpRequestMessage"/> sent to invoke the function
        /// </summary>
        protected Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets/sets a <see cref="Dictionary{TKey, TValue}"/> containing the cookies of the <see cref="HttpRequestMessage"/> sent to invoke the function
        /// </summary>
        protected Dictionary<string, string> Cookies { get; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.InitializeAsync(cancellationToken);
                await this.HttpClient.ConfigureAuthorizationAsync(this.ServiceProvider, this.Authentication, cancellationToken);
                var openApiUriString = this.Function.Operation;
                if (openApiUriString.IsRuntimeExpression())
                {
                    var evaluationResult = (string?)await this.Context.EvaluateAsync(openApiUriString, this.Activity.Input, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(evaluationResult))
                        openApiUriString = evaluationResult;
                }
                var operationComponents = openApiUriString.Split('#');
                if (operationComponents.Length != 2)
                    throw new FormatException($"The 'operation' property of the Open API function with name '{this.Function.Name}' has an invalid value '{this.Function.Operation}'. Open API functions expect a value in the following format: <url_to_openapi_endpoint>#<operation_id>");
                openApiUriString = operationComponents.First();
                if (openApiUriString.IsRuntimeExpression())
                {
                    var evaluationResult = (string?)await this.Context.EvaluateAsync(openApiUriString, this.Activity.Input, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(evaluationResult))
                        openApiUriString = evaluationResult;
                }
                var openApiUri = new Uri(openApiUriString);
                var operationId = operationComponents.Last();
                using (HttpRequestMessage request = new(HttpMethod.Get, openApiUri))
                {
                    using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
                    var responseContent = response.Content == null ? null : await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        this.Logger.LogInformation("Failed to retrieve the Open API document at location '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", openApiUri, response.StatusCode);
                        this.Logger.LogDebug("Response content:\r\n{responseContent}", responseContent ?? "None");
                        response.EnsureSuccessStatusCode(responseContent);
                    }
                    using var responseStream = await response.Content!.ReadAsStreamAsync(cancellationToken)!;
                    this.Document = new OpenApiStreamReader().Read(responseStream, out var diagnostic);
                }
                var operation = this.Document.Paths
                    .SelectMany(p => p.Value.Operations)
                    .FirstOrDefault(o => o.Value.OperationId == operationId);
                if (operation.Value == null)
                    throw new NullReferenceException($"Failed to find an operation with id '{this.Operation}' in OpenAPI document at '{openApiUri}'");
                this.HttpMethod = operation.Key.ToHttpMethod();
                this.Operation = operation.Value;
                this.Servers = this.Document.Servers.Select(s => s.Url).ToList();
                if (!this.Servers.Any())
                    this.Servers.Add(openApiUri.ToString().Replace(openApiUri.PathAndQuery, string.Empty));
                KeyValuePair<string, OpenApiPathItem> path = this.Document.Paths
                    .Single(p => p.Value.Operations.Any(o => o.Value.OperationId == operation.Value.OperationId));
                this.Path = path.Key;
                await this.BuildParametersAsync(cancellationToken);
                var parameters = path.Value.Parameters.ToList();
                parameters.AddRange(this.Operation.Parameters);
                foreach (OpenApiParameter param in parameters
                    .Where(p => p.In == ParameterLocation.Cookie)
                    .GroupBy(p => p.Name))
                {
                    var match = await this.TryGetParameterAsync($@".""{param.Name}""", cancellationToken);
                    if (match.HasMatch)
                        this.Cookies.Add(param.Name, match.Value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                foreach (OpenApiParameter param in parameters
                    .Where(p => p.In == ParameterLocation.Header))
                {
                    var match = await this.TryGetParameterAsync($@".""{param.Name}""", cancellationToken);
                    if (match.HasMatch)
                        this.Headers.Add(param.Name, match.Value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                foreach (OpenApiParameter param in parameters
                    .Where(p => p.In == ParameterLocation.Path))
                {
                    var match = await this.TryGetParameterAsync($@".""{param.Name}""", cancellationToken);
                    if (match.HasMatch)
                        this.Path = this.Path.Replace($"{{{param.Name}}}", match.Value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                Dictionary<string, string> queryParameters = new();
                foreach (OpenApiParameter param in parameters
                    .Where(p => p.In == ParameterLocation.Query))
                {
                    var match = await this.TryGetParameterAsync($@".""{param.Name}""", cancellationToken);
                    if (match.HasMatch)
                        queryParameters.Add(param.Name, match.Value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                this.QueryString = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                if (operation.Value.RequestBody != null)
                {
                    if (operation.Value.RequestBody.Extensions.TryGetValue("x-bodyName", out IOpenApiExtension? bodyNameExtension))
                    {
                        this.Body = await this.Context.EvaluateAsync($".{((OpenApiString)bodyNameExtension).Value}", this.Parameters, cancellationToken);
                    }
                    else
                    {
                        if (this.Parameters.TryGetValue("body", out var bodyValue))
                        {
                            this.Body = await this.Context.EvaluateAsync(".body", this.Parameters, cancellationToken);
                        }
                        else
                        {
                            if(this.Parameters is not ExpandoObject expando)
                            {
                                expando = new ExpandoObject();
                                foreach (var param in this.Parameters)
                                {
                                    expando.TryAdd(param.Key, param.Value);
                                }
                            }
                            this.Body = expando;
                        }  
                    }
                    if (this.Body == null && operation.Value.RequestBody.Required)
                        throw new NullReferenceException($"Failed to determine the required body parameter for the function with name '{this.Function.Name}'");
                }
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <summary>
        /// Builds the parameters to pass to the function to execute
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        protected virtual async Task BuildParametersAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Input == null)
                this.Parameters = new Dictionary<string, object>();
            if (this.FunctionReference.Arguments == null)
                return;
            var jsonInput = JsonConvert.SerializeObject(this.Activity.Input!.ToObject()!, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var jsonArgs = JsonConvert.SerializeObject(this.FunctionReference.Arguments, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            foreach (Match match in Regex.Matches(jsonArgs, @"""\$\{.+?\}"""))
            {
                var expression = match.Value[3..^2].Trim().Replace(@"\""", @"""");
                var evaluationResult = await this.Context.EvaluateAsync(expression, this.Activity.Input!.ToObject()!, cancellationToken);
                if(evaluationResult == null)
                {
                    this.Logger.LogWarning("Failed to evaluate the result of parameter expression '{expression}' on input data '{input}'", expression, jsonInput);
                    continue;
                }
                var valueToken = JToken.FromObject(evaluationResult);
                var value = null as string;
                if (valueToken != null)
                {
                    value = valueToken.Type switch
                    {
                        JTokenType.String => @$"""{valueToken}""",
                        _ => valueToken.ToString(),
                    };
                }
                if (string.IsNullOrEmpty(value))
                    value = "null";
                jsonArgs = jsonArgs.Replace(match.Value, value);
            }
            this.Parameters = JsonConvert.DeserializeObject<ExpandoObject>(jsonArgs)!;
        }

        /// <summary>
        /// Attempts to retrieve the specified parameter
        /// </summary>
        /// <param name="expression">A runtime expression used to reference the parameter to retrieve</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A tuple containing both a boolean indicating if the specified parameter could be found and the parameter's value, if any</returns>
        protected virtual async Task<(bool HasMatch, string Value)> TryGetParameterAsync(string expression, CancellationToken cancellationToken)
        {
            var value = null as string;
            var token = await this.Context.EvaluateAsync(expression, this.Parameters, cancellationToken);
            if (token == null)
                return (false, value!);
            value = token.ToString()!;
            return (true, value);
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await base.ProcessAsync(cancellationToken);
            if (this.Activity.Status == V1WorkflowActivityStatus.Skipped)
                return;
            try
            {
                var output = null as object;
                var success = false;
                foreach (string server in this.Servers)
                {
                    var requestUri = $"{server}{this.Path}";
                    if (requestUri.StartsWith("//"))
                        requestUri = $"https:{requestUri}";
                    if (!string.IsNullOrWhiteSpace(this.QueryString))
                        requestUri += $"?{this.QueryString}";
                    using var request = new HttpRequestMessage(this.HttpMethod, requestUri);
                    foreach (var header in this.Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    request.Headers.Add("Cookie", string.Join(";", this.Cookies.Select(kvp => $"{kvp.Key}={kvp.Value}")));
                    if (this.Body != null)
                    {
                        var json = await this.SerializerProvider.GetJsonSerializers().First().SerializeAsync(this.Body, cancellationToken);
                        request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json); //todo: support other media types?
                    }
                    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        continue;
                    var rawContent = await response.Content.ReadAsByteArrayAsync(cancellationToken)!;
                    var contentString = null as string;
                    if(rawContent != null)
                        contentString = Encoding.UTF8.GetString(rawContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        this.Logger.LogInformation("Failed to execute the Open API operation '{operationId}' at '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", this.Operation.OperationId, response.RequestMessage!.RequestUri, response.StatusCode);
                        this.Logger.LogDebug("Response content:\r\n{responseContent}", contentString ?? "None");
                        response.EnsureSuccessStatusCode(contentString);
                    }
                    if (rawContent!= null)
                    {
                        var mediaType = response.Content?.Headers.ContentType?.MediaType;
                        var serializer = this.SerializerProvider.GetSerializersFor(mediaType).FirstOrDefault();
                        if (serializer == null)
                            throw new NotSupportedException($"Failed to find a serializer for the specified media type '{mediaType}'");
                        using var stream = new MemoryStream(rawContent!);
                        output = (await serializer.DeserializeAsync<JToken>(stream, cancellationToken)).ToObject();
                    }
                    success = true;
                    break;
                }
                if (output == null)
                    output = new();
                if (!success)
                    throw new HttpRequestException($"Failed to execute the operation activity '{this.Operation.OperationId}' (id: '{this.Activity.Id}'): No service available", null, HttpStatusCode.ServiceUnavailable);
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.HttpClient.Dispose();
            base.Dispose(disposing);
        }

    }

}
