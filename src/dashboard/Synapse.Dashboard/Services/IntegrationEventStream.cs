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

using Microsoft.AspNetCore.SignalR.Client;
using Synapse.Apis.Monitoring;
using Synapse.Integration.Models;
using System.Reactive.Subjects;

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IIntegrationEventStream"/> interface
    /// </summary>
    public class IntegrationEventStream
        : IIntegrationEventStream, IDisposable
    {

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="IntegrationEventStream"/>
        /// </summary>
        /// <param name="hubConnection">The current <see cref="Microsoft.AspNetCore.SignalR.Client.HubConnection"/></param>
        public IntegrationEventStream(ILogger<IntegrationEventStream> logger, HubConnection hubConnection)
        {
            this.Logger = logger;
            this.HubConnection = hubConnection;
            this.Subscription = this.HubConnection.On<V1Event>(nameof(ISynapseMonitoringApiClient.PublishIntegrationEvent), this.OnEvent);
        }

        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the current <see cref="Microsoft.AspNetCore.SignalR.Client.HubConnection"/>
        /// </summary>
        protected HubConnection HubConnection { get; }

        /// <summary>
        /// Gets the <see cref="IntegrationEventStream"/>'s subscription
        /// </summary>
        protected IDisposable? Subscription { get; private set; }

        /// <summary>
        /// Gets the <see cref="Subject{T}"/> used to observe <see cref=""/>s consumed by the <see cref="IntegrationEventStream"/>
        /// </summary>
        protected Subject<V1Event> Stream { get; } = new();

        /// <inheritdoc/>
        public virtual IDisposable Subscribe(IObserver<V1Event> observer)
        {
            return this.Stream.Subscribe(observer);
        }

        /// <summary>
        /// Handles the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to handle</param>
        protected virtual void OnEvent(V1Event e)
        {
            this.Stream.OnNext(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Subscription?.Dispose();
                    this.Subscription = null;
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
