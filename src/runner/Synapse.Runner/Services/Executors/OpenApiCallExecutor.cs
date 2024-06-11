// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Neuroglia;
using Neuroglia.Data.Expressions;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> used to execute http <see cref="CallTaskDefinition"/>s using an <see cref="HttpClient"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
/// <param name="httpClientFactory">The service used to create <see cref="HttpClient"/>s</param>
public class OpenApiCallExecutor(IServiceProvider serviceProvider, ILogger<OpenApiCallExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<CallTaskDefinition> context, IJsonSerializer serializer, ISerializerProvider serializerProvider, IHttpClientFactory httpClientFactory)
    : TaskExecutor<CallTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to provide <see cref="ISerializer"/>s
    /// </summary>
    protected ISerializerProvider SerializerProvider { get; } = serializerProvider;

    /// <summary>
    /// Gets the service used to create <see cref="HttpClient"/>s
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;

    /// <summary>
    /// Gets the definition of the OpenAPI call to perform
    /// </summary>
    protected OpenApiCallDefinition? OpenApi { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="OpenApiDocument"/> that defines the OpenAPI operation to call
    /// </summary>
    protected OpenApiDocument? Document { get; set; }

    /// <summary>
    /// Gets the id of the <see cref="OpenApiOperation"/> to call
    /// </summary>
    protected OpenApiOperation? Operation { get; set; }

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpMethod"/> to use to call the <see cref="Operation"/>
    /// </summary>
    protected HttpMethod HttpMethod { get; private set; } = null!;

    /// <summary>
    /// Gets/sets a list containing the addresses of all defined servers, if any
    /// </summary>
    protected List<string> Servers { get; private set; } = null!;

    /// <summary>
    /// Gets/sets a name/value mapping that represents the parameters, if any, to call the OpenAPI operation with
    /// </summary>
    protected IDictionary<string, object>? Parameters { get; private set; }

    /// <summary>
    /// Gets/sets the path segment of the uri that references the OpenAPI operation to call
    /// </summary>
    protected string? Path { get; private set; }

    /// <summary>
    /// Gets/sets the query segment, if any, of the uri that references the OpenAPI operation to call
    /// </summary>
    protected string? Query { get; set; }

    /// <summary>
    /// Gets/sets a name/value mapping of the headers of the <see cref="HttpRequestMessage"/> to call the OpenAPI operation
    /// </summary>
    protected Dictionary<string, string> Headers { get; } = [];

    /// <summary>
    /// Gets/sets a name/value mapping of the cookies, if any, of the <see cref="HttpRequestMessage"/> to call the OpenAPI operation
    /// </summary>
    protected Dictionary<string, string> Cookies { get; } = [];

    /// <summary>
    /// Gets/sets the body of the <see cref="HttpRequestMessage"/> to call the OpenAPI operation
    /// </summary>
    protected object? Body { get; private set; }

    /// <inheritdoc/>
    protected override async Task DoInitializeAsync(CancellationToken cancellationToken)
    {
        this.OpenApi = (OpenApiCallDefinition)this.JsonSerializer.Convert(this.Task.Definition.With, typeof(OpenApiCallDefinition))!;
        using var httpClient = this.HttpClientFactory.CreateClient();
        await httpClient.ConfigureAuthenticationAsync(this.OpenApi.Document.Authentication, this.ServiceProvider, cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, this.OpenApi.Document.Uri);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            this.Logger.LogInformation("Failed to retrieve the OpenAPI document at location '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", this.OpenApi.Document.Uri, response.StatusCode);
            this.Logger.LogDebug("Response content:\r\n{responseContent}", responseContent ?? "None");
            response.EnsureSuccessStatusCode();
        }
        using var responseStream = await response.Content!.ReadAsStreamAsync(cancellationToken)!;
        this.Document = new OpenApiStreamReader().Read(responseStream, out var diagnostic);
        var operation = this.Document.Paths
            .SelectMany(p => p.Value.Operations)
            .FirstOrDefault(o => o.Value.OperationId == this.OpenApi.OperationId);
        if (operation.Value == null) throw new NullReferenceException($"Failed to find an operation with id '{this.OpenApi.OperationId}' in OpenAPI document at '{this.OpenApi.Document.Uri}'");
        this.HttpMethod = operation.Key.ToHttpMethod();
        this.Operation = operation.Value;
        this.Servers = this.Document.Servers.Select(s => s.Url).ToList();
        if (this.Servers.Count == 0) this.Servers.Add(this.OpenApi.Document.Uri.ToString().Replace(this.OpenApi.Document.Uri.PathAndQuery, string.Empty));
        var path = this.Document.Paths.Single(p => p.Value.Operations.Any(o => o.Value.OperationId == operation.Value.OperationId));
        this.Path = path.Key;
        await this.BuildParametersAsync(cancellationToken).ConfigureAwait(false);
        if (this.Parameters == null || this.Parameters.Count < 1) return;
        var parameters = path.Value.Parameters.ToList();
        parameters.AddRange(this.Operation.Parameters);
        foreach (var param in parameters.Where(p => p.In == ParameterLocation.Cookie))
        {
            if (this.Parameters.TryGetValue(param.Name, out var value, StringComparison.OrdinalIgnoreCase) && value != null) this.Cookies.Add(param.Name, value.ToString()!);
            else if (param.Required) throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the OpenAPI operation with id '{this.OpenApi.OperationId}'");
        }
        foreach (var param in parameters.Where(p => p.In == ParameterLocation.Header))
        {
            if (this.Parameters.TryGetValue(param.Name, out var value, StringComparison.OrdinalIgnoreCase) && value != null) this.Headers.Add(param.Name, value.ToString()!);
            else if (param.Required) throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the OpenAPI operation with id '{this.OpenApi.OperationId}'");
        }
        foreach (var param in parameters.Where(p => p.In == ParameterLocation.Path))
        {
            if (this.Parameters.TryGetValue(param.Name, out var value, StringComparison.OrdinalIgnoreCase) && value != null) this.Path = this.Path.Replace($"{{{param.Name}}}", value.ToString());
            else if (param.Required) throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the OpenAPI operation with id '{this.OpenApi.OperationId}'");
        }
        var queryParameters = new Dictionary<string, string>();
        foreach (var param in parameters.Where(p => p.In == ParameterLocation.Query))
        {
            if (this.Parameters.TryGetValue(param.Name, out var value, StringComparison.OrdinalIgnoreCase) && value != null) queryParameters.Add(param.Name, value.ToString()!);
            else if (param.Required) throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the OpenAPI operation with id '{this.OpenApi.OperationId}'");
        }
        this.Query = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        if (operation.Value.RequestBody != null)
        {
            if (operation.Value.RequestBody.Extensions.TryGetValue("x-bodyName", out var bodyNameExtension))
            {
                if (this.Parameters.TryGetValue(((OpenApiString)bodyNameExtension).Value, out var value, StringComparison.OrdinalIgnoreCase)) this.Body = value;
            }
            else
            {
                if (this.Parameters.TryGetValue("body", out var bodyValue))
                {
                    if (this.Parameters.TryGetValue("body", out var value, StringComparison.OrdinalIgnoreCase)) this.Body = value;
                }
                else
                {
                    this.Body = this.Parameters;
                }
            }
            if (this.Body == null && operation.Value.RequestBody.Required) throw new NullReferenceException($"Failed to determine the required body parameter for the OpenAPI operation with id '{this.OpenApi.OperationId}'");
        }
    }

    /// <summary>
    /// Builds the parameters, if any, of the OpenAPI operation to call
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task BuildParametersAsync(CancellationToken cancellationToken = default)
    {
        if (this.OpenApi == null || this.Operation == null) throw new InvalidOperationException("The executor must be initialized before execution");
        if (this.Task.Input == null) this.Parameters = new Dictionary<string, object>();
        if (this.OpenApi.Parameters == null) return;
        this.Parameters = await this.Task.Workflow.Expressions.EvaluateAsync<IDictionary<string, object>>(this.OpenApi.Parameters, this.Task.Input!, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.OpenApi == null || this.Operation == null) throw new InvalidOperationException("The executor must be initialized before execution");
        var output = null as object;
        var success = false;
        foreach (var server in this.Servers)
        {
            var requestUri = $"{server}{this.Path}";
            if (requestUri.StartsWith("//")) requestUri = $"https:{requestUri}";
            if (!string.IsNullOrWhiteSpace(this.Query)) requestUri += $"?{this.Query}";
            using var request = new HttpRequestMessage(this.HttpMethod, requestUri);
            foreach (var header in this.Headers) request.Headers.Add(header.Key, header.Value);
            if (this.Cookies.Count > 0) request.Headers.Add("Cookie", string.Join(";", this.Cookies.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            if (this.Body != null)
            {
                var content = this.Operation.RequestBody.Content.Select(c => new { MediaType = c.Key, Serializer = this.SerializerProvider.GetSerializersFor(c.Key).FirstOrDefault() }).FirstOrDefault(s => s.Serializer != null) 
                    ?? throw new NotSupportedException($"Failed to find a supported content media type for the OpenAPI operation with id '{this.Operation.OperationId}'");
                if (content.Serializer is ITextSerializer textSerializer)
                {
                    var text = textSerializer.SerializeToText(this.Body);
                    request.Content = new StringContent(text, Encoding.UTF8, content.MediaType);
                }
                else
                {
                    var stream = new MemoryStream();
                    content.Serializer.Serialize(this.Body, stream);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    stream.Position = 0;
                    request.Content = new StreamContent(stream);
                    request.Content.Headers.ContentType ??= new(content.MediaType);
                    request.Content.Headers.ContentType.MediaType = content.MediaType;
                }
            }
            using var httpClient = this.HttpClientFactory.CreateClient();
            await httpClient.ConfigureAuthenticationAsync(this.OpenApi.Authentication, this.ServiceProvider, cancellationToken).ConfigureAwait(false);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable) continue;
            var rawContent = await response.Content.ReadAsByteArrayAsync(cancellationToken)!;
            var contentString = string.Empty;
            if (rawContent != null) contentString = Encoding.UTF8.GetString(rawContent);
            if (!response.IsSuccessStatusCode)
            {
                this.Logger.LogError("Failed to execute the OpenAPI operation '{operationId}' at '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", this.Operation.OperationId, response.RequestMessage!.RequestUri, response.StatusCode);
                this.Logger.LogDebug("Response content:\r\n{responseContent}", contentString ?? "None");
                await this.SetErrorAsync(Error.Communication(this.Task.Instance.Reference, (ushort)response.StatusCode, contentString), cancellationToken).ConfigureAwait(false);
                return;
            }
            if (rawContent != null && rawContent.Length > 0)
            {
                var mediaType = response.Content?.Headers.ContentType?.MediaType;
                if (string.IsNullOrWhiteSpace(mediaType))
                {
                    mediaType = MediaTypeNames.Application.Json;
                    this.Logger.LogWarning("Failed to determine the response's content type. Assuming {json}", MediaTypeNames.Application.Json);
                }
                var serializer = this.SerializerProvider.GetSerializersFor(mediaType).FirstOrDefault() ?? throw new NotSupportedException($"Failed to find a serializer for the specified media type '{mediaType}'");
                using var stream = new MemoryStream(rawContent!);
                output = serializer.Deserialize<object>(stream);
                output = this.OpenApi.Output switch
                {
                    HttpOutputFormat.Response => new HttpResponse()
                    {
                        Request = new()
                        {
                            Method = request.Method.Method,
                            Uri = request.RequestUri!,
                            Headers = new(request.Headers.ToDictionary(h => h.Key, h => string.Join(',', h.Value))),
                        },
                        Headers = new(response.Headers.ToDictionary(h => h.Key, h => string.Join(',', h.Value))),
                        StatusCode = (int)response.StatusCode,
                        Content = output
                    },
                    _ => output
                };
            }
            success = true;
            break;
        }
        output ??= new();
        if (!success) throw new HttpRequestException($"Failed to execute the Open APU operation with id '{this.Operation.OperationId}': No service available", null, HttpStatusCode.ServiceUnavailable);
        await this.SetResultAsync(output, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}