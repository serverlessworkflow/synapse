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

using CloudNative.CloudEvents;
using System.Reactive.Subjects;

namespace Synapse.Worker.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IIntegrationEventBus"/> interface
    /// </summary>
    public class IntegrationEventBus
        : IIntegrationEventBus
    {

        private bool _Disposed;

        /// <summary>
        /// Gets the <see cref="Subject"/> used to stream produced <see cref="CloudEvent"/>s
        /// </summary>
        public Subject<CloudEvent> OutboundStream { get; } = new();

        ISubject<CloudEvent> IIntegrationEventBus.OutboundStream => this.OutboundStream;

        /// <summary>
        /// Gets the <see cref="Subject"/> used to stream consumed <see cref="CloudEvent"/>s
        /// </summary>
        public Subject<CloudEvent> InboundStream { get; } = new();

        ISubject<CloudEvent> IIntegrationEventBus.InboundStream => this.InboundStream;

        /// <summary>
        /// Disposes of the <see cref="IntegrationEventBus"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="IntegrationEventBus"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.OutboundStream.Dispose();
                    this.InboundStream.Dispose();
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
