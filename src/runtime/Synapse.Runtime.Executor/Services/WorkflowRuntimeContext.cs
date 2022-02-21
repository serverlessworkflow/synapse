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
using Synapse.Integration.Services;

namespace Synapse.Runtime.Services
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
        /// <param name="synapsePublicApi">The service used to interact with the Synapse Public API</param>
        /// <param name="synapseRuntimeApi">The service used to interact with the Synapse Runtime API</param>
        public WorkflowRuntimeContext(IServiceProvider serviceProvider, ILogger<WorkflowRuntimeContext> logger, IExpressionEvaluatorProvider expressionEvaluatorProvider, ISynapseApi synapsePublicApi, ISynapseRuntimeApi synapseRuntimeApi)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.ExpressionEvaluatorProvider = expressionEvaluatorProvider;
            this.SynapsePublicApi = synapsePublicApi;
            this.SynapseRuntimeApi = synapseRuntimeApi;
            this.ExpressionEvaluator = null!;
            this.Workflow = null!;
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
        public IExpressionEvaluatorProvider ExpressionEvaluatorProvider { get; }

        /// <summary>
        /// Gets the <see cref="IExpressionEvaluator"/> used to evaluate the current workflow's runtime expressions
        /// </summary>
        public IExpressionEvaluator ExpressionEvaluator { get; private set; }

        /// <summary>
        /// Gets the service used to interact with the Synapse Public API
        /// </summary>
        protected ISynapseApi SynapsePublicApi { get; }

        /// <summary>
        /// Gets the service used to interact with the Synapse Runtime API
        /// </summary>
        protected ISynapseRuntimeApi SynapseRuntimeApi { get; }

        /// <inheritdoc/>
        public IWorkflowFacade Workflow { get; private set; }

        public virtual async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                var workflowInstanceId = EnvironmentVariables.Runtime.WorkflowInstanceId.Value;
                this.Logger.LogInformation("Initializing the runtime context for workflow instance with id '{workflowInstanceId}'...", workflowInstanceId);
                var workflowInstance = await this.SynapsePublicApi.GetWorkflowInstanceByIdAsync(workflowInstanceId, cancellationToken);
                if (workflowInstance == null)
                    throw new NullReferenceException($"Failed to find a workflow instance with the specified id '{workflowInstanceId}'");
                this.Logger.LogInformation("Retrieving definition of workflow with id '{workflowInstance.WorkflowId}'...", workflowInstance.WorkflowId);
                var workflow = await this.SynapsePublicApi.GetWorkflowByIdAsync(workflowInstance.WorkflowId, cancellationToken);
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

    }

}
