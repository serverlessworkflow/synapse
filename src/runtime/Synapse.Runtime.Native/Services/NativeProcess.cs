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

using Synapse.Infrastructure.Services;
using System.Diagnostics;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the native implementation of the <see cref="IWorkflowProcess"/>
    /// </summary>
    public class NativeProcess
        : IWorkflowProcess
    {

        /// <inheritdoc/>
        public event EventHandler? Exited;

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="NativeProcess"/>
        /// </summary>
        /// <param name="process">The underlying <see cref="System.Diagnostics.Process"/></param>
        public NativeProcess(Process process) 
        {
            this.Process = process;
            this.Process.Exited += (sender, e) => this.Exited?.Invoke(this, new());
            this.Logs = this.Process.GetLogsAsObservable();
        }

        /// <inheritdoc/>
        public string Id => this.Process.Id.ToString();

        /// <inheritdoc/>
        public ProcessStatus Status { get; protected set; }

        /// <summary>
        /// Gets the underlying <see cref="System.Diagnostics.Process"/>
        /// </summary>
        protected Process Process { get; }

        /// <inheritdoc/>
        public IObservable<string>? Logs { get; }

        /// <inheritdoc/>
        public long? ExitCode => this.Process.HasExited ? this.Process.ExitCode : null;

        /// <inheritdoc/>
        public virtual ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask TerminateAsync(CancellationToken cancellationToken = default)
        {
            this.Process.Close();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Disposes of the <see cref="IWorkflowProcess"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="IWorkflowProcess"/> is being disposed of</param>
        public virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Process.Dispose();
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
        /// Disposes of the <see cref="IWorkflowProcess"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="IWorkflowProcess"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Process.Dispose();
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