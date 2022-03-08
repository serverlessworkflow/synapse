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

using Synapse.Apis.Runtime;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRuntimeProxyFactory"/> interface
    /// </summary>
    public class WorkflowRuntimeProxyFactory
        : IWorkflowRuntimeProxyFactory
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowRuntimeProxyFactory"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        public WorkflowRuntimeProxyFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public virtual IWorkflowRuntimeProxy CreateProxy(string id, IAsyncStreamWriter<RuntimeSignal> signalStream)
        {
            if(string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return ActivatorUtilities.CreateInstance<WorkflowRuntimeProxy>(this.ServiceProvider, id, signalStream);
        }

    }

}
