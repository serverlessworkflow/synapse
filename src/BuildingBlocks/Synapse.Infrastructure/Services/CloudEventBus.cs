using CloudNative.CloudEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Synapse.Configuration;
using System;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Synapse.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ICloudEventBus"/> interface
    /// </summary>
    public class CloudEventBus
        : BackgroundService, ICloudEventBus
    {

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="CloudEventBus"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="formatter">The service used to format <see cref="CloudEvent"/>s</param>
        /// <param name="options">The service used to access the current <see cref="CloudEventBusOptions"/></param>
        /// <param name="subject">The <see cref="Subject{T}"/> used to monitor consumed <see cref="CloudEvent"/>s</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        public CloudEventBus(ILogger<CloudEventBus> logger, ICloudEventFormatter formatter, IOptions<CloudEventBusOptions> options, Subject<CloudEvent> subject, IHttpClientFactory httpClientFactory)
        {
            this.Logger = logger;
            this.Formatter = formatter;
            this.Options = options.Value;
            this.Subject = subject;
            this.HttpClient = httpClientFactory.CreateClient(nameof(CloudEventBus));
            this.Channel = System.Threading.Channels.Channel.CreateUnbounded<CloudEvent>();
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to format <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventFormatter Formatter { get; }

        /// <summary>
        /// Gets the options used to configure the <see cref="CloudEventBus"/>
        /// </summary>
        protected CloudEventBusOptions Options { get; }

        /// <summary>
        /// Gets the <see cref="Subject{T}"/> used to monitor consumed <see cref="CloudEvent"/>s
        /// </summary>
        protected Subject<CloudEvent> Subject { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used to publish <see cref="CloudEvent"/>s
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the <see cref="Channel{T}"/> used to enqueue outgoing <see cref="CloudEvent"/>s
        /// </summary>
        protected Channel<CloudEvent> Channel { get; }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = this.PublishPendingEventsAsync(stoppingToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Publishes pending <see cref="CloudEvent"/>s read from the <see cref="Channel"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task PublishPendingEventsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    CloudEvent e = await this.Channel.Reader.ReadAsync(cancellationToken);
                    if (e == null || cancellationToken.IsCancellationRequested)
                        continue;
                    using HttpResponseMessage response = await this.HttpClient.PostAsync(this.Options.SinkUri, new CloudEventContent(e, ContentMode.Structured, this.Formatter), cancellationToken);
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    //TODO: implement retry policy
                }
            }
        }

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

        /// <summary>
        /// Disposes of the <see cref="CloudEventBus"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not to dispose of the <see cref="CloudEvent"/></param>
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
        public override void Dispose()
        {
            this.Dispose(disposing: true);
        }

    }

}
