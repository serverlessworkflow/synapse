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

using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapse.Integration.Events.WorkflowActivities;
using System.Collections;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process GraphQL <see cref="FunctionDefinition"/>s
    /// </summary>
    public class GraphQLFunctionProcessor
        : FunctionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="GraphQLFunctionProcessor"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="ISerializerProvider"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="serializer">The service used to serialize and deserialize GraphQL requests/responses</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public GraphQLFunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IHttpClientFactory httpClientFactory, IGraphQLWebsocketJsonSerializer serializer, IOptions<ApplicationOptions> options, V1WorkflowActivity activity,
            ActionDefinition action, FunctionDefinition function)
            : base(serviceProvider, loggerFactory, context, activityProcessorFactory, options, activity, action, function)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.Serializer = serializer;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used by the <see cref="FunctionProcessor"/> to execute the function
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the service used to process GraphQL requests
        /// </summary>
        protected IGraphQLClient GraphQLClient { get; private set; } = null!;

        /// <summary>
        /// Gets the service used to serialize and deserialize GraphQL requests/responses
        /// </summary>
        protected IGraphQLWebsocketJsonSerializer Serializer { get; }

        /// <summary>
        /// Gets the <see cref="Uri"/> of the GraphQL endpoint to request
        /// </summary>
        protected Uri EndpointUri { get; private set; } = null!;

        /// <summary>
        /// Gets the name of the GraphQL operation to execute
        /// </summary>
        protected string OperationName { get; private set; } = null!;

        /// <summary>
        /// Gets the type of the GraphQL operation to execute
        /// </summary>
        protected string OperationType { get; private set; } = null!;

        /// <summary>
        /// Gets a string that represents the GraphQL request arguments
        /// </summary>
        protected string? Arguments { get; private set; }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await base.InitializeAsync(cancellationToken);
            await this.HttpClient.ConfigureAuthorizationAsync(this.ServiceProvider, this.Authentication, cancellationToken);
            var operationComponents = this.Function.Operation.Split('#', StringSplitOptions.RemoveEmptyEntries);
            if (operationComponents.Length != 3)
                throw new FormatException($"The 'operation' property of the GraphQL function with name '{this.Function.Name}' has an invalid value '{this.Function.Operation}'. GraphQL functions expect a value in the following format: <url_to_graphql_endpoint>#<literal 'mutation' or 'query'>#<mutation_or_query_field>");
            if (!Uri.TryCreate(operationComponents[0], new(), out var endpointUri)
                || endpointUri == null)
                throw new FormatException($"The value specified as GraphQL endpoint operation component '{operationComponents[0]}' is not a valid uri");
            this.EndpointUri = endpointUri;
            this.OperationType = operationComponents[1].ToLower();
            if (this.OperationType != "query"
                && this.OperationType != "mutation")
                throw new FormatException($"The value specified as GraphQL request type component '{operationComponents[1]}' is not supported");
            this.OperationName = operationComponents[2];
            await this.BuildArgumentsAsync(cancellationToken);
            var options = new GraphQLHttpClientOptions()
            {
                EndPoint = this.EndpointUri
            };
            this.GraphQLClient = new GraphQLHttpClient(options, this.Serializer, this.HttpClient);
        }

        /// <summary>
        /// Builds the GraphQL arguments string
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        protected virtual async ValueTask BuildArgumentsAsync(CancellationToken cancellationToken)
        {
            var args = null as IDictionary<string, object>;
            if (this.Activity.Input == null)
                args = new Dictionary<string, object>();
            if (this.FunctionReference.Arguments == null)
                return;
            string json = JsonConvert.SerializeObject(this.FunctionReference.Arguments, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
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
            args = JsonConvert.DeserializeObject<ExpandoObject>(json)!;
            this.Arguments = string.Join(", ", args.Select(a => $"{a.Key}: {this.SerializeToGraphQL(a.Value)}"));
        }

        /// <summary>
        /// Serializes the specified value to GraphQL
        /// </summary>
        /// <param name="value">The value to serialize to GraphQL</param>
        /// <returns>The serialized value</returns>
        protected virtual string? SerializeToGraphQL(object? value)
        {
            if (value == null)
                return null!;
            var valueType = value.GetType();
            if (valueType.IsPrimitiveType())
                return JsonConvert.SerializeObject(value, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            if (value is IEnumerable enumerable
                && !typeof(IDictionary<string, object>).IsAssignableFrom(valueType))
            {
                var values = new List<string>();
                foreach (var elem in enumerable)
                {
                    values.Add(this.SerializeToGraphQL(elem)!);
                }
                return $"[ {string.Join(", ", values)} ]";
            }
            var properties = new Dictionary<string, string>();
            if (value is IDictionary<string, object> dictionary)
            {
                properties = dictionary.ToDictionary(kvp => kvp.Key, kvp => this.SerializeToGraphQL(kvp.Value))!;
            }
            else
            {
                foreach (var property in value.GetType().GetProperties())
                {
                    properties.Add(property.Name, this.SerializeToGraphQL(property.GetValue(value))!);
                }
            }
            return $"{{{string.Join($", ", properties.Select(p => $"{p.Key}: {p.Value}"))}}}";
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await base.ProcessAsync(cancellationToken);
            if (this.Activity.Status == V1WorkflowActivityStatus.Skipped)
                return;
            var request = new GraphQLRequest()
            {
                Query = @$"{this.OperationType}{{
    {this.OperationName}({this.Arguments}){this.FunctionReference.SelectionSet}
}}"
            };
            try
            {
                var response = null as IGraphQLResponse;
                response = this.OperationType switch
                {
                    "query" => await this.GraphQLClient.SendMutationAsync<dynamic>(request, cancellationToken),
                    "mutation" => await this.GraphQLClient.SendQueryAsync<dynamic>(request, cancellationToken),
                    _ => throw new NotSupportedException($"The specified GraphQL operation type '{this.OperationType}' is not supported"),
                };
                if (response.Errors != null
                    && response.Errors.Any())
                    throw new Exception($"An error occured while executing the GraphQL function with name '{this.Function.Name}':{Environment.NewLine}{string.Join(Environment.NewLine, response.Errors.Select(e => $"{e.Path}: {e.Message}"))}");
                var output = null as object;
                if (response.Data is JToken token)
                    output = token.ToObject();
                else
                    output = response.Data;
                await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
            catch (GraphQLHttpRequestException ex)
            {
                await this.OnErrorAsync(new Exception($"An error occured while executing the GraphQL request: the server returned a non-success status code '{ex.StatusCode}'.{Environment.NewLine}Details: {ex.Content}"), cancellationToken);
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
