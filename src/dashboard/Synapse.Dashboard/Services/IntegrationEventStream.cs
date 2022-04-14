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
