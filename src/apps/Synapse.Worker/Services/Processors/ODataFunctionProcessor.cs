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

using Simple.OData.Client;
using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Worker.Services.Processors
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
        public ODataFunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
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
        /// Gets the service used to query the remote ODATA API
        /// </summary>
        protected IODataClient ODataClient { get; private set; } = null!;

        /// <summary>
        /// Gets the <see cref="Uri"/> of the OData service to query
        /// </summary>
        protected Uri ServiceUri { get; private set; } = null!;

        /// <summary>
        /// Gets the OData entity set to query
        /// </summary>
        protected string EntitySet { get; private set; } = null!;

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await base.InitializeAsync(cancellationToken);
            await this.HttpClient.ConfigureAuthorizationAsync(this.ServiceProvider, this.Authentication, cancellationToken);
            var components = this.Function.Operation.Split("#", StringSplitOptions.RemoveEmptyEntries);
            if (components.Length != 2)
                throw new FormatException($"The 'operation' property of the ODATA function with name '{this.Function.Name}' has an invalid value '{this.Function.Operation}'. ODATA functions expect a value in the following format: <URI_to_odata_service>#<Entity_Set_Name>");
            this.ServiceUri = new(components.First());
            this.EntitySet = components.Last();
            this.HttpClient.BaseAddress = new($"{this.ServiceUri.Scheme}://{this.ServiceUri.Authority}");
            this.ODataClient = new ODataClient(new ODataClientSettings(this.HttpClient, this.HttpClient.BaseAddress.MakeRelativeUri(this.ServiceUri)) { PayloadFormat = ODataPayloadFormat.Json });
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await base.ProcessAsync(cancellationToken);
            if (this.Activity.Status == V1WorkflowActivityStatus.Skipped)
                return;
            try
            {
                var commandOptions = this.FunctionReference.Arguments?.ToObject<ODataCommandOptions>();
                var command = this.EntitySet;
                if (!string.IsNullOrWhiteSpace(commandOptions?.Key))
                    command += $"({commandOptions.Key})";
                if (commandOptions?.QueryOptions != null)
                    command += $"?{commandOptions.QueryOptions.ToQueryString()}";
                var output = null as object;
                if (string.IsNullOrWhiteSpace(commandOptions?.Key))
                    output = await this.ODataClient.FindEntriesAsync(command, cancellationToken);
                else
                    output = await this.ODataClient.FindEntryAsync(command, cancellationToken);
                if (output == null)
                    output = new();
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
        }

    }

}
