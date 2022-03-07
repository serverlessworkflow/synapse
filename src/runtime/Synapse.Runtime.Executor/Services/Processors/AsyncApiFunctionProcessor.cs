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

using Neuroglia.AsyncApi.Client.Services;
using Neuroglia.AsyncApi.Models;

namespace Synapse.Runtime.Executor.Services.Processors
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
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="asyncApiClientFactory">The service used to create <see cref="IAsyncApiClient"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
        public AsyncApiFunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IAsyncApiClientFactory asyncApiClientFactory, IOptions<ApplicationOptions> options, V1WorkflowActivityDto activity,
            ActionDefinition action, FunctionDefinition function)
            : base(serviceProvider, loggerFactory, context, activityProcessorFactory, options, activity, action, function)
        {
            this.AsyncApiClientFactory = asyncApiClientFactory;
        }

        /// <summary>
        /// Gets the service used to create <see cref="IAsyncApiClient"/>s
        /// </summary>
        protected IAsyncApiClientFactory AsyncApiClientFactory { get; }

        /// <summary>
        /// Gets the <see cref="Neuroglia.AsyncApi.Models.AsyncApiDocument"/> that defines the operation to invoke
        /// </summary>
        protected AsyncApiDocument AsyncApiDocument { get; set; } = null!;

        /// <summary>
        /// Gets the service used to interact with the remote Async API
        /// </summary>
        protected IAsyncApiClient AsyncApiClient { get; set; } = null!;

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await base.InitializeAsync(cancellationToken);
            var components = this.Function.Operation.Split('#', StringSplitOptions.RemoveEmptyEntries);

            this.AsyncApiClient = this.AsyncApiClientFactory.CreateClient(this.AsyncApiDocument);
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await base.ProcessAsync(cancellationToken);

        }

    }

}
