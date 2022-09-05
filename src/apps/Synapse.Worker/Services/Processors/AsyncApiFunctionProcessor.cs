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

using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Client;
using Neuroglia.AsyncApi.Client.Services;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapse.Integration.Events.WorkflowActivities;
using System.Dynamic;
using System.Reactive;
using System.Text.RegularExpressions;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process Async API <see cref="FunctionDefinition"/>s
    /// </summary>
    public class AsyncApiFunctionProcessor
        : FunctionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="AsyncApiFunctionProcessor"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="ISerializerProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="httpClientFactory">The service used to created <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="asyncApiReader">The service used to read <see cref="AsyncApiDocument"/>s</param>
        /// <param name="asyncApiClientFactory">The service used to create <see cref="IAsyncApiClient"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public AsyncApiFunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IHttpClientFactory httpClientFactory, IAsyncApiDocumentReader asyncApiReader, IAsyncApiClientFactory asyncApiClientFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity,
            ActionDefinition action, FunctionDefinition function)
            : base(serviceProvider, loggerFactory, context, activityProcessorFactory, options, activity, action, function)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.AsyncApiReader = asyncApiReader;
            this.AsyncApiClientFactory = asyncApiClientFactory;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used by the <see cref="FunctionProcessor"/> to execute the function
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the service used to read <see cref="AsyncApiDocument"/>s
        /// </summary>
        protected IAsyncApiDocumentReader AsyncApiReader { get; }

        /// <summary>
        /// Gets the service used to create <see cref="IAsyncApiClient"/>s
        /// </summary>
        protected IAsyncApiClientFactory AsyncApiClientFactory { get; }

        /// <summary>
        /// Gets the <see cref="Neuroglia.AsyncApi.Models.AsyncApiDocument"/> that defines the operation to invoke
        /// </summary>
        protected AsyncApiDocument Document { get; set; } = null!;

        /// <summary>
        /// Gets the key of the Async API channel the operation to execute belongs to
        /// </summary>
        protected string ChannelKey { get; set; } = null!;

        /// <summary>
        /// Gets the Async API channel the operation to execute belongs to
        /// </summary>
        protected ChannelDefinition Channel { get; set; } = null!;

        /// <summary>
        /// Gets the Async API operation to execute
        /// </summary>
        protected OperationDefinition Operation { get; set; } = null!;

        /// <summary>
        /// Gets the type of the Async API operation to execute
        /// </summary>
        protected OperationType OperationType { get; set; }

        /// <summary>
        /// Gets the payload to use when invoking the remote Async API operation to process
        /// </summary>
        protected object? Payload { get; set; }

        /// <summary>
        /// Gets the service used to interact with the remote Async API
        /// </summary>
        protected IAsyncApiClient AsyncApiClient { get; set; } = null!;

        /// <summary>
        /// Gets an <see cref="IDisposable"/> that represents the <see cref="AsyncApiFunctionProcessor"/>'s subscription to messages publishes by the Async API operation to process
        /// </summary>
        protected IDisposable? Subscription { get; set; }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await base.InitializeAsync(cancellationToken);
            var components = this.Function.Operation.Split('#', StringSplitOptions.RemoveEmptyEntries);
            if (components.Length != 2)
                throw new FormatException($"The 'operation' property of the Async API function with name '{this.Function.Name}' has an invalid value '{this.Function.Operation}'. Async API functions expect a value in the following format: <url_to_asyncapi_endpoint>#<operation_id>");
            var asyncApiUri = components[0];
            var operationId = components[1];
            try
            {
                using (HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, asyncApiUri))
                {
                    using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        this.Logger.LogInformation("Failed to retrieve the Async API document at location '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", asyncApiUri, response.StatusCode);
                        this.Logger.LogDebug("Response content:/r/n{responseContent}", response.Content == null ? "None" : responseContent);
                        response.EnsureSuccessStatusCode(responseContent);
                    }
                    using Stream responseStream = await response.Content!.ReadAsStreamAsync(cancellationToken)!;
                    this.Document = await this.AsyncApiReader.ReadAsync(responseStream, cancellationToken);
                }
                var channel = this.Document.Channels
                    .FirstOrDefault(c => c.Value.DefinesOperationWithId(operationId));
                this.ChannelKey = channel.Key;
                this.Channel = channel.Value;
                if(this.Channel == null)
                    throw new NullReferenceException($"Failed to find an operation with id '{this.Operation}' in the specified AsyncAPI document");
                this.Operation = this.Channel.GetOperationById(operationId);
                this.OperationType = this.Channel.Publish == this.Operation ? OperationType.Publish : OperationType.Subscribe;
                await this.BuildPayloadAsync(cancellationToken);
                this.AsyncApiClient = this.AsyncApiClientFactory.CreateClient(this.Document);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }

        }

        /// <summary>
        /// Builds the payload to use when invoking the remote Async API operation to process
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        protected virtual async Task BuildPayloadAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Input == null)
                this.Payload = new Dictionary<string, object>();
            if (this.FunctionReference.Arguments == null)
                return;
            var json = JsonConvert.SerializeObject(this.FunctionReference.Arguments, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            foreach (Match match in Regex.Matches(json, @"""\$\{.+?\}"""))
            {
                var expression = match.Value[3..^2].Trim();
                var evaluationResult = await this.Context.EvaluateAsync(expression, this.Activity.Input!.ToObject()!, cancellationToken);
                if (evaluationResult == null)
                {
                    Console.WriteLine($"Evaluation result of expression {expression} on data {JsonConvert.SerializeObject(this.Activity.Input)} is NULL"); //todo: replace with better message
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
                json = json.Replace(match.Value, value);
            }
            this.Payload = JsonConvert.DeserializeObject<ExpandoObject>(json)!;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await base.ProcessAsync(cancellationToken);
            try
            {
                switch (this.OperationType)
                {
                    case OperationType.Publish:
                        var message = new MessageBuilder()
                            .WithPayload(this.Payload)
                            .Build();
                        await this.AsyncApiClient.PublishAsync(this.ChannelKey, message, cancellationToken);
                        await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, new()), cancellationToken);
                        await this.OnCompletedAsync(cancellationToken);
                        break;
                    case OperationType.Subscribe:
                        this.Subscription = await this.AsyncApiClient.SubscribeToAsync(this.ChannelKey, Observer.Create<IMessage>(this.OnMessageAsync), cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(OperationType)} '{this.OperationType}' is not supported");
                }
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the specified <see cref="IMessage"/>
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> to handle</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async void OnMessageAsync(IMessage message)
        {
            try
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message));
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, message.Payload), this.CancellationTokenSource.Token);
                await this.OnCompletedAsync(this.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, this.CancellationTokenSource.Token);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.HttpClient.Dispose();
                this.Subscription?.Dispose();
            }
            base.Dispose(disposing);
        }

    }

}
