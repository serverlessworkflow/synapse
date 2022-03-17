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

using Neuroglia.Serialization;
using Synapse.Apis.Runtime;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowRuntimeProxy"/>
    /// </summary>
    public class WorkflowRuntimeProxy
        : IWorkflowRuntimeProxy
    {

        /// <inheritdoc/>
        public event EventHandler? Disposed;

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="WorkflowRuntimeProxy"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        /// <param name="id">The <see cref="WorkflowRuntimeProxy"/>'s id, which is the same than the id of the executed workflow instance </param>
        /// <param name="signalStream">The <see cref="WorkflowRuntimeProxy"/>'s <see cref="RuntimeSignal"/> stream</param>
        public WorkflowRuntimeProxy(ILogger<WorkflowRuntimeProxy> logger, IMediator mediator, IMapper mapper, string id, IAsyncStreamWriter<RuntimeSignal> signalStream)
        {
            this.Logger = logger;
            this.Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            if(string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            this.Id = id;
            this.SignalStream = signalStream ?? throw new ArgumentNullException(nameof(signalStream));
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <summary>
        /// Gets the <see cref="WorkflowRuntimeProxy"/>'s connection
        /// </summary>
        protected IAsyncStreamWriter<RuntimeSignal> SignalStream { get; }

        /// <inheritdoc/>
        public virtual async Task CorrelateAsync(V1CorrelationContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            await this.SignalStream.WriteAsync(new(SignalType.Correlate, Dynamic.FromObject(context)), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SuspendAsync(CancellationToken cancellationToken = default)
        {
            await this.SignalStream.WriteAsync(new(SignalType.Suspend), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
        {
            await this.SignalStream.WriteAsync(new(SignalType.Cancel), cancellationToken);
        }

        /// <summary>
        /// Disposes of the <see cref="WorkflowRuntimeProxy"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="WorkflowRuntimeProxy"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                    this.Disposed?.Invoke(this, new());
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
