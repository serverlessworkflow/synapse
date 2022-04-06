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
    /// Defines the fundamentals of a service used to manage <see cref="IWorkflowRuntimeProxy"/> instances
    /// </summary>
    public interface IWorkflowRuntimeProxyManager
    {

        /// <summary>
        /// Registers the specified <see cref="IWorkflowRuntimeProxy"/>
        /// </summary>
        /// <param name="runtimeProxy">The <see cref="IWorkflowRuntimeProxy"/> to register</param>
        /// <returns>The registered <see cref="IWorkflowRuntimeProxy"/></returns>
        IWorkflowRuntimeProxy Register(IWorkflowRuntimeProxy runtimeProxy);

        /// <summary>
        /// Gets the <see cref="IWorkflowRuntimeProxy"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="IWorkflowRuntimeProxy"/> to get</param>
        /// <returns>The <see cref="IWorkflowRuntimeProxy"/> with the specified id</returns>
        IWorkflowRuntimeProxy GetProxy(string id);

        /// <summary>
        /// Unregisters the specified <see cref="IWorkflowRuntimeProxy"/>
        /// </summary>
        /// <param name="runtimeProxy">The <see cref="IWorkflowRuntimeProxy"/> to unregister</param>
        void Unregister(IWorkflowRuntimeProxy runtimeProxy);

    }

}
