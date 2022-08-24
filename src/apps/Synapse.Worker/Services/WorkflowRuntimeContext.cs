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
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Data.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapse.Apis.Management;
using Synapse.Apis.Runtime;
using Synapse.Worker.Services;
using System.Text.RegularExpressions;

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRuntimeContext"/>
    /// </summary>
    public class WorkflowRuntimeContext
        : IWorkflowRuntimeContext
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowRuntimeContext"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="expressionEvaluatorProvider">The service used to create provide <see cref="IExpressionEvaluator"/>s</param>
        /// <param name="secretManager">The service used to manage secrets</param>
        /// <param name="managementApi">The service used to interact with the Synapse Public API</param>
        /// <param name="runtimeApi">The service used to interact with the Synapse Runtime API</param>
        public WorkflowRuntimeContext(IServiceProvider serviceProvider, ILogger<WorkflowRuntimeContext> logger, IExpressionEvaluatorProvider expressionEvaluatorProvider, 
            ISecretManager secretManager, ISynapseManagementApi managementApi, ISynapseRuntimeApi runtimeApi)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.ExpressionEvaluatorProvider = expressionEvaluatorProvider;
            this.SecretManager = secretManager;
            this.ManagementApi = managementApi;
            this.RuntimeApi = runtimeApi;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to create provide <see cref="IExpressionEvaluator"/>s
        /// </summary>
        protected IExpressionEvaluatorProvider ExpressionEvaluatorProvider { get; }

        /// <summary>
        /// Gets the <see cref="IExpressionEvaluator"/> used to evaluate the current workflow's runtime expressions
        /// </summary>
        protected IExpressionEvaluator ExpressionEvaluator { get; private set; } = null!;

        /// <summary>
        /// Gets the service used to manage secrets
        /// </summary>
        protected ISecretManager SecretManager { get; }

        /// <summary>
        /// Gets the service used to interact with the Synapse Public API
        /// </summary>
        protected ISynapseManagementApi ManagementApi { get; }

        /// <summary>
        /// Gets the service used to interact with the Synapse Runtime API
        /// </summary>
        protected ISynapseRuntimeApi RuntimeApi { get; }

        /// <inheritdoc/>
        public IWorkflowFacade Workflow { get; private set; } = null!;

        /// <inheritdoc/>
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

        /// <inheritdoc/>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                var workflowInstanceId = EnvironmentVariables.Runtime.WorkflowInstanceId.Value;
                this.Logger.LogInformation("Initializing the runtime context for workflow instance with id '{workflowInstanceId}'...", workflowInstanceId);
                var workflowInstance = await this.ManagementApi.GetWorkflowInstanceByIdAsync(workflowInstanceId, cancellationToken);
                if (workflowInstance == null)
                    throw new NullReferenceException($"Failed to find a workflow instance with the specified id '{workflowInstanceId}'");
                this.Logger.LogInformation("Retrieving definition of workflow with id '{workflowInstance.WorkflowId}'...", workflowInstance.WorkflowId);
                var workflow = await this.ManagementApi.GetWorkflowByIdAsync(workflowInstance.WorkflowId, cancellationToken);
                if (workflow == null)
                    throw new NullReferenceException($"Failed to find a workflow with the specified id '{workflowInstance.WorkflowId}'");
                this.ExpressionEvaluator = this.ExpressionEvaluatorProvider.GetEvaluator(workflow.Definition.ExpressionLanguage)!;
                if (this.ExpressionEvaluator == null)
                    throw new NullReferenceException($"Failed to find an expression evaluator for language '{workflow.Definition.ExpressionLanguage}'");
                this.Workflow = ActivatorUtilities.CreateInstance<WorkflowFacade>(this.ServiceProvider, workflowInstance, workflow.Definition);
                this.Logger.LogInformation("Runtime context initialized");
            }
            catch(Exception ex)
            {
                this.Logger.LogError("An error occured while initializing the workflow runtime context: {ex}", ex.ToString());
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<object?> EvaluateAsync(string runtimeExpression, object? data, CancellationToken cancellationToken)
        {
            runtimeExpression = runtimeExpression.Trim();
            if (runtimeExpression.StartsWith("${"))
                runtimeExpression = runtimeExpression[2..^1].Trim();
            var args = await this.BuildRuntimExpressionArgumentsAsync(cancellationToken);
            foreach (Match functionMatch in Regex.Matches(runtimeExpression, @"(fn:\w*)"))
            {
                var functionName = functionMatch.Value.Trim();
                functionName = functionName[3..];
                if (!this.Workflow.Definition.TryGetFunction(functionName, out var function))
                    throw new NullReferenceException($"Failed to find a function with the specified name '{functionName}' in the workflow '{this.Workflow}'");
                if (function.Type != FunctionType.Expression)
                    throw new InvalidOperationException($"The function with name '{function.Name}' is of type '{EnumHelper.Stringify(function.Type)}' and cannot be called in an expression");
                var value = this.ExpressionEvaluator.Evaluate(function.Operation, data!, args);
                var serializedValue = null as string;
                if (value != null)
                    serializedValue = JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                runtimeExpression = runtimeExpression.Replace(functionMatch.Value, serializedValue);
            }
            return this.ExpressionEvaluator.Evaluate(runtimeExpression, data!, args);
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetSecretAsync<T>(string secret, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));
            var secrets = await this.SecretManager.GetSecretsAsync(cancellationToken);
            if (!secrets.TryGetValue(secret, out var secretValue))
                throw new NullReferenceException($"Failed to find the specified secret '{secret}'");
            return secretValue switch
            {
                T t => t,
                DynamicObject dyn => dyn.ToObject<T>(),
                JObject jobj => jobj.ToObject<T>()!,
                _ => throw new InvalidCastException(),
            };
        }

        /// <summary>
        /// Builds the runtime expression arguments
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="IDictionary{TKey, TValue}"/> that represents the runtime expression arguments</returns>
        protected virtual async Task<IDictionary<string, object>> BuildRuntimExpressionArgumentsAsync(CancellationToken cancellationToken)
        {
            var args = new Dictionary<string, object>
            {
                { "WORKFLOW", await this.BuildRuntimeExpressionWorkflowArgumentAsync(cancellationToken) },
                { "CONST", await this.BuildRuntimeExpressionConstantsArgumentAsync(cancellationToken) },
                { "SECRETS", await this.BuildRuntimeExpressionSecretsArgumentAsync(cancellationToken) }
            };
            return args;
        }

        /// <summary>
        /// Builds the runtime expression '$WORKFLOW' argument object
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The runtime expression '$WORKFLOW' argument object</returns>
        protected virtual async Task<object> BuildRuntimeExpressionWorkflowArgumentAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(new
            {
                workflow = new
                {
                    id = this.Workflow.Definition.GetUniqueIdentifier(),
                    instanceId = this.Workflow.Instance.Id
                }
            });
        }

        /// <summary>
        /// Builds the runtime expression '$CONST' argument object
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The runtime expression '$CONST' argument object</returns>
        protected virtual async Task<object> BuildRuntimeExpressionConstantsArgumentAsync(CancellationToken cancellationToken)
        {
            var constants = this.Workflow.Definition.Constants?.ToObject();
            if (constants == null)
                constants = new();
            return await Task.FromResult(constants);
        }

        /// <summary>
        /// Builds the runtime expression '$SECRETS' argument object
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The runtime expression '$SECRETS' argument object</returns>
        protected virtual async Task<object> BuildRuntimeExpressionSecretsArgumentAsync(CancellationToken cancellationToken)
        {
            if (await this.SecretManager.GetSecretsAsync(cancellationToken) is not object secrets)
                secrets = new();
            return secrets;
        }

    }

}
