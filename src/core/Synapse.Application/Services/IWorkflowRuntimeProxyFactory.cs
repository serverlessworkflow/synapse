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
    /// Defines the fundamentals of a service used to create <see cref="IWorkflowRuntimeProxy"/>
    /// </summary>
    public interface IWorkflowRuntimeProxyFactory
    {

        /// <summary>
        /// Creates a new <see cref="IWorkflowRuntimeProxy"/>
        /// </summary>
        /// <param name="id">The id of the workflow instance to create a new <see cref="IWorkflowRuntimeProxy"/> for</param>
        /// <param name="signalStream">The service used to write <see cref="RuntimeSignal"/>s to the stream</param>
        /// <returns>A new <see cref="IWorkflowRuntimeProxy"/></returns>
        IWorkflowRuntimeProxy CreateProxy(string id, IAsyncStreamWriter<RuntimeSignal> signalStream);

    }

}
