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
using System.Reactive.Linq;

namespace Synapse.Runtime.Services
{
    /// <summary>
    /// Represents the Docker implementation of the <see cref="IWorkflowProcess"/> interface
    /// </summary>
    public class DockerProcess
        : ProcessBase
    {

        /// <summary>
        /// Initializes a a new <see cref="DockerProcess"/>
        /// </summary>
        /// <param name="id">The <see cref="DockerProcess"/>'s id</param>
        /// <param name="docker">The service used to interact with the Docker API</param>
        public DockerProcess(string id, IDockerClient docker)
            : base(id)
        {
            this.Docker = docker;
        }

        private IObservable<string>? _Logs;
        /// <inheritdoc/>
        public override IObservable<string> Logs => this._Logs!;

        private long? _ExitCode;
        /// <inheritdoc/>
        public override long? ExitCode => this._ExitCode;

        /// <summary>
        /// Gets the service used to interact with the Docker API
        /// </summary>
        protected IDockerClient Docker { get; }

        /// <summary>
        /// Gets the <see cref="DockerProcess"/>'s <see cref="System.Threading.CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; } = new();

        /// <inheritdoc/>
        public override async ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            await this.Docker.Containers.StartContainerAsync(this.Id, new(), cancellationToken);
            var progress = new Progress<string>();
            await this.Docker.Containers.GetContainerLogsAsync(Id, new() { Follow = true, ShowStdout = true, ShowStderr = true }, this.CancellationTokenSource.Token, progress);
            this._Logs = Observable.FromEventPattern<string?>(handler => progress.ProgressChanged += handler, handler => progress.ProgressChanged -= handler)
                .Where(l => !string.IsNullOrWhiteSpace(l.EventArgs))
                .Select(l => l.EventArgs!);
            _ = Task.Run(async () => await this.WaitForExitAsync());
        }

        /// <inheritdoc/>
        public override async ValueTask TerminateAsync(CancellationToken cancellationToken = default)
        {
            await this.Docker.Containers.StopContainerAsync(this.Id, new(), cancellationToken);
            this.CancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Waits for the Docker container to complete
        /// </summary>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task WaitForExitAsync()
        {
            while (!this.CancellationTokenSource.IsCancellationRequested)
            {
                var response = await this.Docker.Containers.WaitContainerAsync(this.Id, this.CancellationTokenSource.Token);
                this._ExitCode = response.StatusCode;
                this.OnExited();
            }
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync(bool disposing)
        {
            this.CancellationTokenSource.Dispose();
            return base.DisposeAsync(disposing);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.CancellationTokenSource.Dispose();
            base.Dispose(disposing);
        }

    }

}