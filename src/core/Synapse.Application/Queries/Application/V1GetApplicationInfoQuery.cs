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

using Neuroglia.Data.Expressions;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Infrastructure.Plugins;
using Synapse.Integration.Models;
using System.Collections;
using System.Runtime.InteropServices;

namespace Synapse.Application.Queries.Application
{

    /// <summary>
    /// Represents the <see cref="IQuery"/> used to retrieve information about the running Synapse instance
    /// </summary>
    public class V1GetApplicationInfoQuery
        : Query<V1ApplicationInfo>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetApplicationInfoQuery"/>
        /// </summary>
        public V1GetApplicationInfoQuery()
        {

        }

    }

    /// <summary>
    /// Represents the service used to handle <see cref="V1GetApplicationInfoQuery"/> instances
    /// </summary>
    public class V1GetApplicationInfoQueryHandler
        : QueryHandlerBase,
        IQueryHandler<V1GetApplicationInfoQuery, V1ApplicationInfo>
    {

        /// <summary>
        /// Initializes a new <see cref="V1GetApplicationInfoQueryHandler"/>
        /// </summary>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="workflowRuntime">The current <see cref="IWorkflowRuntime"/></param>
        /// <param name="pluginManager">The service used to manage <see cref="IPlugin"/>s</param>
        /// <param name="expressionEvaluators">An <see cref="IEnumerable"/> containing all registered <see cref="IExpressionEvaluator"/>s</param>
        public V1GetApplicationInfoQueryHandler(IHostEnvironment environment, ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper, 
            IWorkflowRuntime workflowRuntime, IPluginManager pluginManager, IEnumerable<IExpressionEvaluator> expressionEvaluators) 
            : base(loggerFactory, mediator, mapper)
        {
            this.Environment = environment;
            this.WorkflowRuntime = workflowRuntime;
            this.PluginManager = pluginManager;
            this.ExpressionEvaluators = expressionEvaluators;
        }

        /// <summary>
        /// Gets the current <see cref="IHostEnvironment"/>
        /// </summary>
        protected IHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the service used to manage <see cref="IPlugin"/>s
        /// </summary>
        protected IPluginManager PluginManager { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowRuntime"/>
        /// </summary>
        protected IWorkflowRuntime WorkflowRuntime { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable"/> containing all registered <see cref="IExpressionEvaluator"/>s
        /// </summary>
        protected IEnumerable<IExpressionEvaluator> ExpressionEvaluators { get; }

        /// <inheritdoc/>
        public virtual async Task<IOperationResult<V1ApplicationInfo>> HandleAsync(V1GetApplicationInfoQuery query, CancellationToken cancellationToken = default)
        {
            var name = this.Environment.ApplicationName;
            var version = typeof(V1GetApplicationInfoQuery).Assembly.GetName().Version!.ToString();
            var osDescription = RuntimeInformation.OSDescription;
            var frameworkDescription = RuntimeInformation.FrameworkDescription;
            var serverlessWorkflowSdkVersion = typeof(WorkflowDefinition).Assembly.GetName().Version!.ToString();
            var environmentName = this.Environment.EnvironmentName;
            var workflowRuntimeName = this.WorkflowRuntime.GetType().Name.Replace("Runtime", string.Empty);
            var supportedRuntimeExpressionLanguages = new[] { "jq" }; //todo: need to provide a list based on IExpressionEvaluators. The latter interface should be edited to return an array of supported languages
            var environmentVariables = System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process)
                .OfType<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value!.ToString()!);
            var plugins = this.PluginManager.Plugins.Select(p => p.Metadata);
            var info = new V1ApplicationInfo(name, version, osDescription, frameworkDescription, serverlessWorkflowSdkVersion, environmentName,
                workflowRuntimeName, supportedRuntimeExpressionLanguages, environmentVariables, plugins);
            return await Task.FromResult(this.Ok(info));
        }

    }

}
