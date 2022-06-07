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

using Synapse.Application.Services;
using Synapse.Infrastructure.Services;
using System.Diagnostics;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the native implementation of the <see cref="IWorkflowProcess"/>
    /// </summary>
    public class NativeProcess
        : WorkflowProcessBase
    {

        /// <summary>
        /// Initializes a new <see cref="NativeProcess"/>
        /// </summary>
        /// <param name="process">The underlying <see cref="System.Diagnostics.Process"/></param>
        public NativeProcess(Process process) 
        {
            this.Id = Guid.NewGuid().ToString();
            this.Process = process;
            this.Process.Exited += (sender, e) => this.OnExited();
            this.Logs = this.Process.GetLogsAsObservable();
        }

        /// <inheritdoc/>
        public override string Id { get; }

        /// <summary>
        /// Gets the underlying <see cref="System.Diagnostics.Process"/>
        /// </summary>
        protected Process Process { get; }

        /// <inheritdoc/>
        public override IObservable<string> Logs { get; }

        /// <inheritdoc/>
        public override long? ExitCode => this.Process.HasExited ? this.Process.ExitCode : null;

        /// <inheritdoc/>
        public override ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override ValueTask TerminateAsync(CancellationToken cancellationToken = default)
        {
            this.Process.Close();
            return ValueTask.CompletedTask;
        }

    }

}