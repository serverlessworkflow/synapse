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

using Neuroglia.Serialization;
using Simple.OData.Client;
using Synapse.Integration.Events.WorkflowActivities;
using System.Text;

namespace Synapse.Runtime.Executor.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process ODATA <see cref="FunctionDefinition"/>s
    /// </summary>
    public class ODataFunctionProcessor
        : FunctionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="ODataFunctionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public ODataFunctionProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IHttpClientFactory httpClientFactory, ISerializerProvider serializerProvider, IOptions<ApplicationOptions> options, V1WorkflowActivityDto activity,
            ActionDefinition action, FunctionDefinition function)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, action, function)
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
        /// Gets the <see cref="Uri"/> of the OData service to query
        /// </summary>
        protected Uri ServiceUri { get; private set; } = null!;

        /// <summary>
        /// Gets the OData entity set to query
        /// </summary>
        protected string EntitySet { get; private set; } = null!;

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            var components = this.Function.Operation.Split("#", StringSplitOptions.RemoveEmptyEntries);
            if (components.Length <= 2)
                throw new FormatException($"The 'operation' property of the ODATA function with name '{this.Function.Name}' has an invalid value '{this.Function.Operation}'. ODATA functions expect a value in the following format: <URI_to_odata_service>#<Entity_Set_Name>");
            this.ServiceUri = new(components.First());
            this.EntitySet = components.Last();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = new ODataClient();
                await client.FindEntriesAsync("Packages?$filter=Title eq 'Simple.OData.Client'");

                var commandOptions = this.FunctionReference.Arguments?.ToObject<ODataCommandOptions>();

                var uri = $"{this.ServiceUri}/{this.EntitySet}";
                if (commandOptions?.QueryOptions != null)
                    uri += $"?{commandOptions.QueryOptions.ToQueryString()}";
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                using var response = await this.HttpClient.SendAsync(request, cancellationToken);
                var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
                var rawContent = await response.Content.ReadAsByteArrayAsync(cancellationToken)!;
                var contentString = null as string;
                if (rawContent != null)
                    contentString = Encoding.UTF8.GetString(rawContent);
                if (!response.IsSuccessStatusCode)
                {
                    this.Logger.LogInformation("Failed to execute the ODATA function '{functionName}' at '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", this.Function.Name, response.RequestMessage!.RequestUri, response.StatusCode);
                    this.Logger.LogDebug("Response content:/r/n{responseContent}", contentString == null ? "None" : contentString);
                    response.EnsureSuccessStatusCode();
                }
                if (rawContent != null)
                {
                    var mediaType = response.Content?.Headers.ContentType?.MediaType;
                    var serializer = this.SerializerProvider.GetSerializersFor(mediaType).FirstOrDefault();
                    if (serializer == null)
                        throw new NotSupportedException($"Failed to find a serializer for the specified media type '{mediaType}'");
                    using var stream = new MemoryStream(rawContent!);
                    output = await serializer.DeserializeAsync<ExpandoObject>(stream, cancellationToken);
                }
                if (output == null)
                    output = new();
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
            
        }
    }

}
