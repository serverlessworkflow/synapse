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
    /// Represents a base class for all implementations of the <see cref="IWorkflowProcess"/> interface
    /// </summary>
    public abstract class WorkflowProcessBase
        : IWorkflowProcess
    {

        /// <inheritdoc/>
        public event EventHandler? Exited;
        /// <inheritdoc/>
        public event EventHandler? Disposed;

        private bool _Disposed;

        /// <inheritdoc/>
        public abstract string Id { get; }

        /// <inheritdoc/>
        public ProcessStatus Status { get; protected set; }

        /// <inheritdoc/>
        public abstract IObservable<string> Logs { get; }

        /// <inheritdoc/>
        public abstract long? ExitCode { get; }

        /// <inheritdoc/>
        public abstract ValueTask StartAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract ValueTask TerminateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles the <see cref="WorkflowProcessBase"/>'s having exited
        /// </summary>
        protected virtual void OnExited()
        {
            this.Status = ProcessStatus.Exited;
            this.Exited?.Invoke(this, new());
        }

        /// <summary>
        /// Disposes of the <see cref="WorkflowProcessBase"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowProcessBase"/> is being disposed of</param>
        public virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Disposed?.Invoke(this, new());
                }
                this._Disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.DisposeAsync(true);
        }

        /// <summary>
        /// Disposes of the <see cref="WorkflowProcessBase"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowProcessBase"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Disposed?.Invoke(this, new());
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}
