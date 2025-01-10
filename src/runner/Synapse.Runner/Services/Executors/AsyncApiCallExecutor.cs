﻿// Copyright © 2024-Present The Synapse Authors
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
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Client;
using Neuroglia.AsyncApi.Client.Services;
using Neuroglia.AsyncApi.IO;
using Neuroglia.AsyncApi.v3;
using Neuroglia.Data.Expressions;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> used to execute AsyncAPI <see cref="CallTaskDefinition"/>s using an <see cref="HttpClient"/>
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="schemaHandlerProvider">The service used to provide <see cref="Core.Infrastructure.Services.ISchemaHandler"/> implementations</param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="httpClientFactory">The service used to create <see cref="HttpClient"/>s</param>
/// <param name="asyncApiDocumentReader">The service used to read <see cref="IAsyncApiDocument"/>s</param>
/// <param name="asyncApiClientFactory">The service used to create <see cref="IAsyncApiClient"/>s</param>
public class AsyncApiCallExecutor(IServiceProvider serviceProvider, ILogger<AsyncApiCallExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory,
    ITaskExecutionContext<CallTaskDefinition> context, Core.Infrastructure.Services.ISchemaHandlerProvider schemaHandlerProvider, IJsonSerializer serializer, IHttpClientFactory httpClientFactory, IAsyncApiDocumentReader asyncApiDocumentReader, IAsyncApiClientFactory asyncApiClientFactory)
    : TaskExecutor<CallTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, schemaHandlerProvider, serializer)
{

    /// <summary>
    /// Gets the service used to create <see cref="HttpClient"/>s
    /// </summary>
    protected IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;

    /// <summary>
    /// Gets the service used to read <see cref="IAsyncApiDocument"/>s
    /// </summary>
    protected IAsyncApiDocumentReader AsyncApiDocumentReader { get; } = asyncApiDocumentReader;

    /// <summary>
    /// Gets the service used to create <see cref="IAsyncApiClient"/>s
    /// </summary>
    protected IAsyncApiClientFactory AsyncApiClientFactory { get; } = asyncApiClientFactory;

    /// <summary>
    /// Gets the definition of the AsyncAPI call to perform
    /// </summary>
    protected AsyncApiCallDefinition? AsyncApi { get; set; }

    /// <summary>
    /// Gets/sets the <see cref="IAsyncApiDocument"/> that defines the AsyncAPI operation to call
    /// </summary>
    protected V3AsyncApiDocument? Document { get; set; }

    /// <summary>
    /// Gets the <see cref="V3OperationDefinition"/> to call
    /// </summary>
    protected KeyValuePair<string, V3OperationDefinition> Operation { get; set; }

    /// <summary>
    /// Gets an object used to describe the credentials, if any, used to authenticate a user agent with the AsyncAPI application
    /// </summary>
    protected AuthorizationInfo? Authorization { get; set; }

    /// <summary>
    /// Gets/sets the payload, if any, of the message to publish, in case the <see cref="Operation"/>'s <see cref="V3OperationDefinition.Action"/> has been set to <see cref="V3OperationAction.Receive"/>
    /// </summary>
    protected object? MessagePayload { get; set; }

    /// <summary>
    /// Gets/sets the headers, if any, of the message to publish, in case the <see cref="Operation"/>'s <see cref="V3OperationDefinition.Action"/> has been set to <see cref="V3OperationAction.Receive"/>
    /// </summary>
    protected object? MessageHeaders { get; set; }

    /// <inheritdoc/>
    protected override async Task DoInitializeAsync(CancellationToken cancellationToken)
    {
        this.AsyncApi = (AsyncApiCallDefinition)this.JsonSerializer.Convert(this.Task.Definition.With, typeof(AsyncApiCallDefinition))!;
        using var httpClient = this.HttpClientFactory.CreateClient();
        await httpClient.ConfigureAuthenticationAsync(this.AsyncApi.Document.Endpoint.Authentication, this.ServiceProvider, this.Task.Workflow.Definition, cancellationToken).ConfigureAwait(false);
        var uriString = StringFormatter.NamedFormat(this.AsyncApi.Document.EndpointUri.OriginalString, this.Task.Input.ToDictionary());
        if (uriString.IsRuntimeExpression()) uriString = await this.Task.Workflow.Expressions.EvaluateAsync<string>(uriString, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(uriString)) throw new NullReferenceException("The AsyncAPI endpoint URI cannot be null or empty");
        if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri) || uri == null) throw new Exception($"Failed to parse the specified string '{uriString}' into a new URI");
        using var request = new HttpRequestMessage(HttpMethod.Get, uriString);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            this.Logger.LogInformation("Failed to retrieve the AsyncAPI document at location '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", uri, response.StatusCode);
            this.Logger.LogDebug("Response content:\r\n{responseContent}", responseContent ?? "None");
            response.EnsureSuccessStatusCode();
        }
        using var responseStream = await response.Content!.ReadAsStreamAsync(cancellationToken)!;
        var document = await this.AsyncApiDocumentReader.ReadAsync(responseStream, cancellationToken).ConfigureAwait(false);
        if (document is not V3AsyncApiDocument v3Document) throw new NotSupportedException("Synapse only supports AsyncAPI v3.0.0 at the moment");
        this.Document = v3Document;
        if (string.IsNullOrWhiteSpace(this.AsyncApi.Operation)) throw new NullReferenceException("The 'operation' parameter must be set when performing an AsyncAPI v3 call");
        var operationId = this.AsyncApi.Operation;
        if (operationId.IsRuntimeExpression()) operationId = await this.Task.Workflow.Expressions.EvaluateAsync<string>(operationId, this.Task.Input, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(operationId)) throw new NullReferenceException("The operation ref cannot be null or empty");
        this.Operation = this.Document.Operations.FirstOrDefault(o => o.Key == operationId);
        if (this.Operation.Value == null) throw new NullReferenceException($"Failed to find an operation with id '{operationId}' in AsyncAPI document at '{uri}'");
        if (this.AsyncApi.Authentication != null) this.Authorization = await AuthorizationInfo.CreateAsync(this.AsyncApi.Authentication, this.ServiceProvider, this.Task.Workflow.Definition, cancellationToken).ConfigureAwait(false);
        switch (this.Operation.Value.Action)
        {
            case V3OperationAction.Receive:
                await this.BuildMessagePayloadAsync(cancellationToken).ConfigureAwait(false);
                await this.BuildMessageHeadersAsync(cancellationToken).ConfigureAwait(false);
                break;
            case V3OperationAction.Send: break;
            default: throw new NotSupportedException($"The specified operation action '{this.Operation.Value.Action}' is not supported");
        }
    }

    /// <summary>
    /// Builds the payload, if any, of the message to publish, in case the <see cref="Operation"/>'s <see cref="V3OperationDefinition.Action"/> has been set to <see cref="V3OperationAction.Receive"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task BuildMessagePayloadAsync(CancellationToken cancellationToken = default)
    {
        if (this.AsyncApi == null || this.Operation.Value == null) throw new InvalidOperationException("The executor must be initialized before execution");
        if (this.Task.Input == null) this.MessagePayload = new { };
        if (this.AsyncApi.Message?.Payload == null) return;
        var arguments = this.GetExpressionEvaluationArguments();
        if (this.Authorization != null)
        {
            arguments ??= new Dictionary<string, object>();
            arguments.Add("authorization", this.Authorization);
        }
        this.MessagePayload = await this.Task.Workflow.Expressions.EvaluateAsync<object>(this.AsyncApi.Message.Payload, this.Task.Input!, arguments, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the headers, if any, of the message to publish, in case the <see cref="Operation"/>'s <see cref="V3OperationDefinition.Action"/> has been set to <see cref="V3OperationAction.Receive"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task BuildMessageHeadersAsync(CancellationToken cancellationToken = default)
    {
        if (this.AsyncApi == null || this.Operation.Value == null) throw new InvalidOperationException("The executor must be initialized before execution");
        if (this.AsyncApi.Message?.Headers == null) return;
        var arguments = this.GetExpressionEvaluationArguments();
        if (this.Authorization != null)
        {
            arguments ??= new Dictionary<string, object>();
            arguments.Add("authorization", this.Authorization);
        }
        this.MessageHeaders = await this.Task.Workflow.Expressions.EvaluateAsync<object>(this.AsyncApi.Message.Headers, this.Task.Input!, arguments, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.AsyncApi == null || this.Document == null || this.Operation.Value == null) throw new InvalidOperationException("The executor must be initialized before execution");
        return this.Operation.Value.Action switch
        {
            V3OperationAction.Receive => this.DoExecutePublishOperationAsync(cancellationToken),
            V3OperationAction.Send => this.DoExecuteSubscribeOperationAsync(cancellationToken),
            _ => throw new NotSupportedException($"The specified operation action '{this.Operation.Value.Action}' is not supported"),
        };
    }

    /// <summary>
    /// Executes an AsyncAPI publish operation
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task DoExecutePublishOperationAsync(CancellationToken cancellationToken)
    {
        if (this.AsyncApi == null || this.Document == null || this.Operation.Value == null) throw new InvalidOperationException("The executor must be initialized before execution");
        await using var asyncApiClient = this.AsyncApiClientFactory.CreateFor(this.Document);
        var parameters = new AsyncApiPublishOperationParameters(this.Operation.Key, this.AsyncApi.Server, this.AsyncApi.Protocol)
        {
            Payload = this.MessagePayload,
            Headers = this.MessageHeaders
        };
        await using var result = await asyncApiClient.PublishAsync(parameters, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccessful) throw new Exception("Failed to execute the AsyncAPI publish operation");
        await this.SetResultAsync(null, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an AsyncAPI subscribe operation
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task DoExecuteSubscribeOperationAsync(CancellationToken cancellationToken)
    {
        if (this.AsyncApi == null || this.Document == null || this.Operation.Value == null) throw new InvalidOperationException("The executor must be initialized before execution");
        if (this.AsyncApi.Subscription == null) throw new NullReferenceException("The 'subscription' must be set when performing an AsyncAPI v3 subscribe operation");
        await using var asyncApiClient = this.AsyncApiClientFactory.CreateFor(this.Document);
        var parameters = new AsyncApiSubscribeOperationParameters(this.Operation.Key, this.AsyncApi.Server, this.AsyncApi.Protocol);
        await using var result = await asyncApiClient.SubscribeAsync(parameters, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccessful) throw new Exception("Failed to execute the AsyncAPI subscribe operation");
        if(result.Messages == null)
        {
            await this.SetResultAsync(null, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
            return;
        }
        var observable = result.Messages;
        if (this.AsyncApi.Subscription.Consume.For != null) observable = observable.TakeUntil(Observable.Timer(this.AsyncApi.Subscription.Consume.For.ToTimeSpan()));
        if (this.AsyncApi.Subscription.Consume.Amount.HasValue) observable = observable.Take(this.AsyncApi.Subscription.Consume.Amount.Value);
        else if (!string.IsNullOrWhiteSpace(this.AsyncApi.Subscription.Consume.While)) observable = observable.Select(message => Observable.FromAsync(async () =>
        {
            var keepGoing = await this.Task.Workflow.Expressions.EvaluateConditionAsync(this.AsyncApi.Subscription.Consume.While, this.Task.Input!,this.GetExpressionEvaluationArguments(),cancellationToken).ConfigureAwait(false);
            return (message, keepGoing);
        })).Concat().TakeWhile(i => i.keepGoing).Select(i => i.message);
        else if (!string.IsNullOrWhiteSpace(this.AsyncApi.Subscription.Consume.Until)) observable = observable.Select(message => Observable.FromAsync(async () =>
        {
            var keepGoing = !(await this.Task.Workflow.Expressions.EvaluateConditionAsync(this.AsyncApi.Subscription.Consume.Until, this.Task.Input!, this.GetExpressionEvaluationArguments(), cancellationToken).ConfigureAwait(false));
            return (message, keepGoing);
        })).Concat().TakeWhile(i => i.keepGoing).Select(i => i.message);
        var messages = await observable.ToAsyncEnumerable().ToListAsync(cancellationToken).ConfigureAwait(false);
        await this.SetResultAsync(messages, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
    }

}