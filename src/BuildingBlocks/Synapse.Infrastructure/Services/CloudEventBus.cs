using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Synapse.Services
{

    public class CloudEventBus
        : ICloudEventBus
    {

        private bool _Disposed;

        public CloudEventBus(ILogger<CloudEventBus> logger, Subject<CloudEvent> subject, IHttpClientFactory httpClientFactory)
        {
            this.Logger = logger;
            this.Subject = subject;
            this.HttpClient = httpClientFactory.CreateClient(nameof(CloudEventBus));
        }

        protected ILogger Logger { get; }

        protected Subject<CloudEvent> Subject { get; }

        protected HttpClient HttpClient { get; }

        protected Channel<CloudEvent> Channel { get; }

        /// <inheritdoc/>
        public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
        {
            await this.Channel.Writer.WriteAsync(e, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IDisposable Subscribe(IObserver<CloudEvent> observer)
        {
            return this.Subject.Subscribe(observer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                    this.HttpClient.Dispose();
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
