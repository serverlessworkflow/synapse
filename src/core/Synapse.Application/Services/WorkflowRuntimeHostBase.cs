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

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the base class for all <see cref="IWorkflowRuntime"/> implementations
    /// </summary>
    public abstract class WorkflowRuntimeHostBase
        : BackgroundService, IWorkflowRuntime
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowRuntimeHostBase"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        protected WorkflowRuntimeHostBase(ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
        }
        
        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc/>
        public abstract Task<IWorkflowProcess> CreateProcessAsync(V1Workflow workflow, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await this.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the <see cref="WorkflowRuntimeHostBase"/>
        /// </summary>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

    }

}
