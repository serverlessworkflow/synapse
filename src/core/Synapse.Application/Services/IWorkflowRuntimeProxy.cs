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
    /// Defines the fundamentals of a service used to proxy a workflow runtime
    /// </summary>
    public interface IWorkflowRuntimeProxy
        : IDisposable
    {

        /// <summary>
        /// The event fired whenever the <see cref="IWorkflowRuntimeProxy"/> has been disposed of
        /// </summary>
        event EventHandler? Disposed;

        /// <summary>
        /// Gets the <see cref="IWorkflowRuntimeProxy"/>'s id, which is the same than the id of the executed workflow instance 
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Peforms runtime correlation in the specified <see cref="V1CorrelationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="V1CorrelationContext"/> in which to perform the correlation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task CorrelateAsync(V1CorrelationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Suspends the <see cref="IWorkflowRuntimeProxy"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SuspendAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels the <see cref="IWorkflowRuntimeProxy"/>'s execution
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task CancelAsync(CancellationToken cancellationToken = default);

    }

}
