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

namespace Synapse.Core.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a container
/// </summary>
public interface IContainer
    : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Gets the container's standard output stream
    /// </summary>
    StreamReader? StandardOutput { get; }

    /// <summary>
    /// Gets the container's standard error stream
    /// </summary>
    StreamReader? StandardError { get; }

    /// <summary>
    /// Gets the container's exit code
    /// </summary>
    long? ExitCode { get; }

    /// <summary>
    /// Starts the container
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for the container to exit
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task WaitForExitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the container
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task StopAsync(CancellationToken cancellationToken = default);

}
