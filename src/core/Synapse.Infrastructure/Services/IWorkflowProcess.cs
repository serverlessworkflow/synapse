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

namespace Synapse.Infrastructure.Services
{

    /// <summary>
    /// Defines the fundamentals of a workflow process
    /// </summary>
    public interface IWorkflowProcess
        : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Gets the event fired whenever the <see cref="IWorkflowProcess"/> has been disposed of
        /// </summary>
        event EventHandler? Disposed;

        /// <summary>
        /// Gets the event fired whenever the <see cref="IWorkflowProcess"/> has exited
        /// </summary>
        event EventHandler? Exited;

        /// <summary>
        /// Gets the <see cref="IWorkflowProcess"/>'s id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the <see cref="IWorkflowProcess"/>'s status
        /// </summary>
        ProcessStatus Status { get; }

        /// <summary>
        /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="IWorkflowProcess"/>'s logs
        /// </summary>
        IObservable<string> Logs { get; }

        /// <summary>
        /// Gets the <see cref="IWorkflowProcess"/>'s exit code
        /// </summary>
        long? ExitCode { get; }

        /// <summary>
        /// Starts the <see cref="IWorkflowProcess"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        ValueTask StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminates the <see cref="IWorkflowProcess"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        ValueTask TerminateAsync(CancellationToken cancellationToken = default);

    }

}
