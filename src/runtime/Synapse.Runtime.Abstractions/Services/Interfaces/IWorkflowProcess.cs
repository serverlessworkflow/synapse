// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Synapse.Runtime.Services;

/// <summary>
/// Defines the fundamentals of a process that executes a workflow instance
/// </summary>
public interface IWorkflowProcess
    : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Gets the event fired whenever the <see cref="IWorkflowProcess"/> has exited
    /// </summary>
    event EventHandler? Exited;

    /// <summary>
    /// Gets the <see cref="IWorkflowProcess"/>'s id
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="IWorkflowProcess"/>'s STDOUT
    /// </summary>
    IObservable<string>? StandardOutput { get; }

    /// <summary>
    /// Gets an <see cref="IObservable{T}"/> used to observe the <see cref="IWorkflowProcess"/>'s STDERR
    /// </summary>
    IObservable<string>? StandardError { get; }

    /// <summary>
    /// Gets the <see cref="IWorkflowProcess"/>'s exit code
    /// </summary>
    long? ExitCode { get; }

    /// <summary>
    /// Starts the <see cref="IWorkflowProcess"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates the <see cref="IWorkflowProcess"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task StopAsync(CancellationToken cancellationToken = default);

}