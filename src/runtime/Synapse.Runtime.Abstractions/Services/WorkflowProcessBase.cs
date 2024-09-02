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
/// Represents the service used to handle a specific <see cref="Resources.WorkflowInstance"/>'s process
/// </summary>
public abstract class WorkflowProcessBase
    : IWorkflowProcess
{

    bool _disposed;

    /// <inheritdoc/>
    public event EventHandler? Exited;

    /// <inheritdoc/>
    public abstract string Id { get; }

    /// <inheritdoc/>
    public abstract IObservable<string>? StandardOutput { get; }

    /// <inheritdoc/>
    public abstract IObservable<string>? StandardError { get; }

    /// <inheritdoc/>
    public abstract long? ExitCode { get; }

    /// <inheritdoc/>
    public abstract Task StartAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the <see cref="WorkflowProcessBase"/>'s having exited
    /// </summary>
    protected virtual void OnExited() => this.Exited?.Invoke(this, new());

    /// <summary>
    /// Disposes of the <see cref="WorkflowProcessBase"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowProcessBase"/> is being disposed of</param>
    /// <returns>A new awaitable <see cref="ValueTask"/></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed) return;
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="WorkflowProcessBase"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowProcessBase"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed) return;

        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

}
