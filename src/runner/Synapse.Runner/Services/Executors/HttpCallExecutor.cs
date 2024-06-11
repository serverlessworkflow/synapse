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

using Neuroglia;
using Neuroglia.Data.Expressions;
using System.Net.Mime;
using System.Text;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> used to execute http <see cref="CallTaskDefinition"/>s using an <see cref="System.Net.Http.HttpClient"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
/// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
public class HttpCallExecutor(IServiceProvider serviceProvider, ILogger<HttpCallExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<CallTaskDefinition> context, IJsonSerializer serializer, ISerializerProvider serializerProvider, IHttpClientFactory httpClientFactory)
    : TaskExecutor<CallTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the service used to provide <see cref="ISerializer"/>s
    /// </summary>
    protected ISerializerProvider SerializerProvider { get; } = serializerProvider;

    /// <summary>
    /// Gets the service used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClientFactory.CreateClient();

    /// <summary>
    /// Gets the definition of the http call to perform
    /// </summary>
    protected HttpCallDefinition? Http { get; set; }

    /// <inheritdoc/>
    protected override async Task DoInitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.Http = (HttpCallDefinition)this.JsonSerializer.Convert(this.Task.Definition.With, typeof(HttpCallDefinition))!;
            var authentication = this.Http.Endpoint.Authentication == null ? null : await this.Task.Workflow.Expressions.EvaluateAsync<AuthenticationPolicyDefinition>(this.Http.Endpoint.Authentication, this.Task.Input, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
            await this.HttpClient.ConfigureAuthenticationAsync(authentication, this.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            await this.SetErrorAsync(new()
            {
                Status = ErrorStatus.Validation,
                Type = ErrorType.Validation,
                Title = ErrorTitle.Validation,
                Detail = $"Invalid/missing call parameters for function 'http': {ex.Message}"
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.Http == null) throw new InvalidOperationException("The executor must be initialized before execution");
        ISerializer? serializer;
        var defaultMediaType = this.Http.Body is string
            ? MediaTypeNames.Text.Plain
            : MediaTypeNames.Application.Json;
        if ((this.Http.Headers?.TryGetValue("Content-Type", out var mediaType) != true && this.Http.Headers?.TryGetValue("Content-Type", out mediaType) != true) || string.IsNullOrWhiteSpace(mediaType)) mediaType = defaultMediaType;
        else mediaType = mediaType.Split(';', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        var requestContent = (HttpContent?)null;
        if (mediaType.StartsWith("text"))
        {
            var rawRequestContent = this.Http.Body?.ToString();
            if (!string.IsNullOrWhiteSpace(rawRequestContent) && rawRequestContent.IsRuntimeExpression()) rawRequestContent = await this.Task.Workflow.Expressions.EvaluateAsync<string>(rawRequestContent, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(rawRequestContent)) requestContent = new StringContent(rawRequestContent, Encoding.UTF8, mediaType);
        }
        else if (mediaType == MediaTypeNames.Application.Octet)
        {
            if (this.Http.Body != null)
            {
                byte[]? buffer;
                if (this.Http.Body is byte[] byteArray) buffer = byteArray;
                else buffer = Convert.FromBase64String(this.Http.Body.ToString()!);
                if (buffer != null) requestContent = new StreamContent(new MemoryStream(buffer));
            }
        }
        else if (this.Http.Body != null)
        {
            requestContent = this.Http.Body switch
            {
                string stringContent => new StringContent(stringContent, Encoding.UTF8, mediaType),
                byte[] byteArrayContent => new StreamContent(new MemoryStream(byteArrayContent)),
                _ => null
            };
            if (requestContent == null)
            {
                var value = this.Http.Body;
                value = await this.Task.Workflow.Expressions.EvaluateAsync<object?>(value, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken);
                if (value != null)
                {
                    serializer = this.SerializerProvider.GetSerializersFor(mediaType).FirstOrDefault() ?? throw new NotSupportedException($"The specified media type '{mediaType}' is not supported for serialization");
                    if (serializer is ITextSerializer textSerializer)
                    {
                        var text = textSerializer.SerializeToText(value);
                        requestContent = new StringContent(text, Encoding.UTF8, mediaType);
                    }
                    else
                    {
                        var stream = new MemoryStream();
                        serializer.Serialize(value, stream);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                        stream.Position = 0;
                        requestContent = new StreamContent(stream);
                        requestContent.Headers.ContentType ??= new(mediaType);
                        requestContent.Headers.ContentType.MediaType = mediaType;
                    }
                }
            }
        }
        var uri = StringFormatter.NamedFormat(this.Http.Endpoint.Uri.OriginalString, this.Task.Input.ToDictionary());
        if (uri.IsRuntimeExpression()) uri = await this.Task.Workflow.Expressions.EvaluateAsync<string>(uri, this.Task.Input, this.Task.Arguments, cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(new HttpMethod(this.Http.Method), uri) { Content = requestContent };
        using var response = await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) //todo: could be configurable on HTTP call?
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            this.Logger.LogError("Failed to request '{method} {uri}'. The remote server responded with a non-success status code '{statusCode}'.", this.Http.Method, this.Http.Endpoint.Uri, response.StatusCode);
            this.Logger.LogDebug("Response content:\r\n{responseContent}", detail ?? "None");
            await this.SetErrorAsync(Error.Communication(this.Task.Instance.Reference, (ushort)response.StatusCode, detail), cancellationToken).ConfigureAwait(false);
            return;
        }
        var result = (object?)null;
        if (response.Content.Headers.ContentType == null || string.IsNullOrWhiteSpace(response.Content.Headers.ContentType.MediaType))
        {
            result = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            serializer = this.SerializerProvider.GetSerializersFor(response.Content.Headers.ContentType.MediaType).FirstOrDefault();
            if (serializer == null) await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            else
            {
                if (serializer is ITextSerializer textSerializer)
                {
                    var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    result = textSerializer.Deserialize<object>(text);
                }
                else
                {
                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    result = serializer.Deserialize<object>(stream);
                }
            }
        }
        result = this.Http.Output switch
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
                Content = result
            },
            _ => result
        };
        await this.SetResultAsync(result, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing).ConfigureAwait(false);
        this.HttpClient.Dispose();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        this.HttpClient.Dispose();
    }

}
