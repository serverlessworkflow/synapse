// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuroglia.Eventing.CloudEvents;
using Neuroglia.Serialization;
using Polly;
using Synapse.Api.Application.Configuration;
using System.Text;
using System.Threading.Channels;

namespace Synapse.Api.Application.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ICloudEventPublisher"/> interface
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="options">The service used to access the current <see cref="ApiServerOptions"/></param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class CloudEventPublisher(ILogger<CloudEventPublisher> logger, IJsonSerializer jsonSerializer, IOptions<ApiServerOptions> options, HttpClient httpClient)
    : IHostedService, ICloudEventPublisher, IDisposable, IAsyncDisposable
{

    bool _disposed;

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the current <see cref="ApiServerOptions"/>
    /// </summary>
    protected ApiServerOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets the service used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClient;

    /// <summary>
    /// Gets the <see cref="CloudEventPublisher"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    /// Gets the <see cref="Channel{T}"/> used to enqueue <see cref="CloudEvent"/>s to publish
    /// </summary>
    protected Channel<CloudEvent> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<CloudEvent>();

    /// <inheritdoc/>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.Value.CloudEvents.Endpoint == null) logger.LogWarning("No endpoint configured for cloud events. Events will not be published.");
        else _ = this.PublishEnqueuedEventsAsync();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(e);
        await this.Channel.Writer.WriteAsync(e, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        this.CancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Publishes enqueued <see cref="CloudEvent"/>s
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task PublishEnqueuedEventsAsync()
    {
        var policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .WrapAsync(Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(5)));
        while (!this.CancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var e = await this.Channel.Reader.ReadAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
                if (e == null)
                {
                    await Task.Delay(10);
                    continue;
                }
                var json = this.JsonSerializer.SerializeToText(e);
                using var content = new StringContent(json, Encoding.UTF8, CloudEventContentType.Json);
                using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.CloudEvents.Endpoint) { Content = content };
                using var response = await policy.ExecuteAsync(async () => await this.HttpClient.SendAsync(request, this.CancellationTokenSource.Token).ConfigureAwait(false));
                json = await response.Content.ReadAsStringAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("An error occurred while publishing the cloud event with id '{eventId}' to the configure endpoint '{endpoint}': {ex}", e.Id, options.Value.CloudEvents.Endpoint, json);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An exception occurred while publishing cloud events");
                //todo: start persisting following events to disk
            }
        }
    }

    /// <summary>
    /// Disposes of the <see cref="CloudEventPublisher"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="CloudEventPublisher"/> is being disposed of</param>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed) return;
        if (disposing)
        {
            this.CancellationTokenSource.Dispose();
        }
        this._disposed = true;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the <see cref="CloudEventPublisher"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="CloudEventPublisher"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed) return;
        if (disposing)
        {
            this.CancellationTokenSource.Dispose();
        }
        this._disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
