using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Synapse.Runner.Application.Services.Processors
{

    /// <summary>
    /// Represents an <see cref="ActionProcessor"/> used to process <see cref="ServerlessWorkflow.Sdk.Models.FunctionReference"/>s
    /// </summary>
    public class FunctionProcessor
        : ActionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="FunctionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public FunctionProcessor(ILoggerFactory loggerFactory, IWorkflowExecutionContext executionContext, IWorkflowActivityProcessorFactory activityProcessorFactory, IHttpClientFactory httpClientFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, ActionDefinition action, FunctionDefinition function)
            : base(loggerFactory, executionContext, activityProcessorFactory, options, activity, action)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.Function = function;
        }

        /// <summary>
        /// Gets the <see cref="FunctionDefinition"/> to process
        /// </summary>
        protected FunctionDefinition Function { get; }

        /// <summary>
        /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.FunctionReference"/> to process
        /// </summary>
        protected FunctionReference FunctionReference
        {
            get
            {
                return this.Action.Function;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used by the <see cref="FunctionProcessor"/> to execute the function
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets/sets the <see cref="Microsoft.OpenApi.Models.OpenApiDocument"/> that declares the operation to invoke
        /// </summary>
        protected OpenApiDocument OpenApiDocument { get; set; }

        /// <summary>
        /// Gets/sets the <see cref="Microsoft.OpenApi.Models.OpenApiOperation"/> that describes the <see cref="FunctionDefinition"/> to invoke
        /// </summary>
        protected OpenApiOperation OpenApiOperation { get; set; }

        /// <summary>
        /// Gets/sets a <see cref="List{T}"/> containing the addresses of the available remote servers
        /// </summary>
        protected List<string> Servers { get; set; }

        /// <summary>
        /// Gets/sets the OpenApi id of the operation to invoke
        /// </summary>
        protected string OperationId { get; set; }

        /// <summary>
        /// Gets/sets the <see cref="System.Net.Http.HttpMethod"/> to use when executing the operation
        /// </summary>
        protected HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// Gets/sets a <see cref="JObject"/> that represents the parameters to pass to the function to execute
        /// </summary>
        protected JObject Parameters { get; set; }

        /// <summary>
        /// Gets/sets the uri's path segment of the function to execute
        /// </summary>
        protected string Path { get; set; }

        /// <summary>
        /// Gets/sets the uri's query string segment of the function to execute
        /// </summary>
        protected string QueryString { get; set; }

        /// <summary>
        /// Gets/sets the body of the <see cref="HttpRequestMessage"/> sent to invoke the function
        /// </summary>
        protected JToken Body { get; set; }

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
            string[] operationComponents = this.Function.Operation.Split('#');
            Uri openApiUri = new(operationComponents.First());
            this.OperationId = operationComponents.Last();
            try
            {
                using (HttpRequestMessage request = new(HttpMethod.Get, openApiUri))
                {
                    using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        this.Logger.LogInformation("Failed to retrieve the Open API document at location '{uri}'. The remote server responded with a non-success status code '{statusCode}'.");
                        this.Logger.LogDebug($"Response content:{Environment.NewLine}{{responseContent}}", response.Content == null ? "None" : await response.Content.ReadAsStringAsync(cancellationToken));
                        response.EnsureSuccessStatusCode();
                    }
                    using Stream responseStream = await response.Content?.ReadAsStreamAsync(cancellationToken);
                    this.OpenApiDocument = new OpenApiStreamReader().Read(responseStream, out OpenApiDiagnostic diagnostic);
                }
                KeyValuePair<OperationType, OpenApiOperation> operation = this.OpenApiDocument.Paths
                    .SelectMany(p => p.Value.Operations)
                    .FirstOrDefault(o => o.Value.OperationId == this.OperationId);
                if (operation.Value == null)
                    throw new NullReferenceException($"Failed to find an operation with id '{this.OperationId}' in OpenAPI document at '{openApiUri}'");
                this.HttpMethod = operation.Key.ToHttpMethod();
                this.OpenApiOperation = operation.Value;
                this.Servers = this.OpenApiDocument.Servers.Select(s => s.Url).ToList();
                if (!this.Servers.Any())
                    this.Servers.Add(openApiUri.ToString().Replace(openApiUri.PathAndQuery, string.Empty));
                KeyValuePair<string, OpenApiPathItem> path = this.OpenApiDocument.Paths
                    .Single(p => p.Value.Operations.Contains(operation));
                this.Path = path.Key;
                foreach (OpenApiParameter param in this.OpenApiOperation.Parameters
                    .Where(p => p.In == ParameterLocation.Cookie)
                    .GroupBy(p => p.Name))
                {
                    if (this.TryGetParameter($@".""{param.Name}""", out string value) && param.Required)
                        this.Cookies.Add(param.Name, value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                this.BuildParameters();
                foreach (OpenApiParameter param in this.OpenApiOperation.Parameters
                    .Where(p => p.In == ParameterLocation.Header))
                {
                    if (this.TryGetParameter($@".""{param.Name}""", out string value) && param.Required)
                        this.Headers.Add(param.Name, value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                foreach (OpenApiParameter param in this.OpenApiOperation.Parameters
                    .Where(p => p.In == ParameterLocation.Path))
                {
                    if (this.TryGetParameter($@".""{param.Name}""", out string value) && param.Required)
                        this.Path = this.Path.Replace($"{{{param.Name}}}", value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                Dictionary<string, string> queryParameters = new();
                foreach (OpenApiParameter param in this.OpenApiOperation.Parameters
                    .Where(p => p.In == ParameterLocation.Query))
                {
                    if (this.TryGetParameter($".{param.Name}", out string value) && param.Required)
                        queryParameters.Add(param.Name, value);
                    else if (param.Required)
                        throw new NullReferenceException($"Failed to find the definition of the required parameter '{param.Name}' in the function definition with name '{this.Function.Name}'");
                }
                this.QueryString = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                if (operation.Value.RequestBody != null)
                {
                    if (operation.Value.RequestBody.Extensions.TryGetValue("x-bodyName", out IOpenApiExtension bodyNameExtension))
                    {
                        this.Body = this.ExecutionContext.ExpressionEvaluator.Evaluate($".{((OpenApiString)bodyNameExtension).Value}", this.Parameters);
                    }
                    else
                    {
                        if (this.Parameters.Count == 1)
                        {
                            this.Body = this.Parameters.Properties().First().Value;
                        }
                        else if (this.Parameters.TryGetValue("body", out JToken bodyValue))
                        {
                            this.Body = this.ExecutionContext.ExpressionEvaluator.Evaluate(".body", this.Parameters);
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
        protected virtual void BuildParameters()
        {
            if (this.Activity.Data == null)
                this.Parameters = new JObject();
            if (this.FunctionReference.Arguments == null)
                return;
            string json = this.FunctionReference.Arguments.ToString();
            foreach (Match match in Regex.Matches(json, @"""\$\{.+?\}"""))
            {
                string expression = match.Value[3..^2].Trim();
                JToken valueToken = this.ExecutionContext.ExpressionEvaluator.Evaluate(expression, this.Activity.Data);
                string value = null;
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
            this.Parameters = JObject.Parse(json);
        }

        /// <summary>
        /// Attempts to get the value of the parameter at the specified path
        /// </summary>
        /// <param name="expression">The path to the parameter to get the value of</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A boolean indicating whether or not the parameter could be fetched</returns>
        protected virtual bool TryGetParameter(string expression, out string value)
        {
            value = null;
            JToken token = this.ExecutionContext.ExpressionEvaluator.Evaluate(expression, this.Parameters);
            if (token == null)
                return false;
            value = token.ToString();
            return true;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                JToken output = null;
                bool success = false;
                foreach (string server in this.Servers)
                {
                    string requestUri = $"{server}{this.Path}";
                    if (requestUri.StartsWith("//"))
                        requestUri = $"https:{requestUri}";
                    if (!string.IsNullOrWhiteSpace(this.QueryString))
                        requestUri += $"?{this.QueryString}";
                    HttpRequestMessage request = new(this.HttpMethod, requestUri);
                    foreach (KeyValuePair<string, string> header in this.Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    request.Headers.Add("Cookie", string.Join(";", this.Cookies.Select(kvp => $"{kvp.Key}={kvp.Value}")));
                    if (this.Body != null)
                        request.Content = new StringContent(this.Body.ToString(), Encoding.UTF8, MediaTypeNames.Application.Json);
                    using (request)
                    {
                        using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
                        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            continue;
                        string rawContent = await response.Content?.ReadAsStringAsync(cancellationToken);
                        if (!response.IsSuccessStatusCode)
                        {
                            this.Logger.LogInformation("Failed to execute the REST operation '{operationId}' at '{uri}'. The remote server responded with a non-success status code '{statusCode}'.", this.OperationId, response.RequestMessage.RequestUri, response.StatusCode);
                            this.Logger.LogDebug($"Response content:{Environment.NewLine}{{responseContent}}", response.Content == null ? "None" : await response.Content.ReadAsStringAsync(cancellationToken), rawContent);
                            response.EnsureSuccessStatusCode();
                        }
                        if (!string.IsNullOrWhiteSpace(rawContent))
                        {
                            switch (response.Content?.Headers.ContentType?.MediaType)
                            {
                                case MediaTypeNames.Application.Json:
                                    output = JToken.Parse(rawContent);
                                    break;
                                case MediaTypeNames.Application.Xml:
                                    XmlDocument doc = new();
                                    doc.LoadXml(rawContent);
                                    string json = JsonConvert.SerializeXmlNode(doc);
                                    output = JToken.Parse(json);
                                    break;
                                default:
                                    if (response.Content.Headers.TryGetValues("Content-Disposition", out IEnumerable<string> contentDispositionValues)
                                        && contentDispositionValues.FirstOrDefault() == "attachment")
                                    {
                                        string fileName = contentDispositionValues.First(cdv => cdv.StartsWith("filename", StringComparison.InvariantCultureIgnoreCase));

                                    }
                                    break;
                            }
                        }
                        success = true;
                        break;
                    }
                }
                if (output == null)
                    output = new JObject();
                if (!success)
                    throw new HttpRequestException($"Failed to execute the operation execution pointer '{this.OperationId}' (id: '{this.Activity.Id}'): No service available", null, HttpStatusCode.ServiceUnavailable);
                await this.OnResultAsync(V1WorkflowExecutionResult.Next(output), cancellationToken);
                await this.OnCompletedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await this.OnErrorAsync(ex, cancellationToken);
            }
        }

    }

}
