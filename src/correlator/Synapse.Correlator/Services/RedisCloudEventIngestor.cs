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

using StackExchange.Redis;

namespace Synapse.Correlator.Services;

/// <summary>
/// Represents a service used to ingest <see cref="CloudEvent"/>s using Redis
/// </summary>
/// <param name="logger">The service used to perform logging</param>
/// <param name="connectionMultiplexer">The service used to connect to a Redis API</param>
/// <param name="cloudEventBus">The service used to stream input and output <see cref="CloudEvent"/>s</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize data to/from JSON</param>
/// <param name="options">The service used to access the current <see cref="CorrelatorOptions"/></param>
public class RedisCloudEventIngestor(ILogger<RedisCloudEventIngestor> logger, IConnectionMultiplexer connectionMultiplexer, ICloudEventBus cloudEventBus, IJsonSerializer jsonSerializer, IOptions<CorrelatorOptions> options)
    : IHostedService
{

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the service used to connect to a Redis API
    /// </summary>
    protected IConnectionMultiplexer ConnectionMultiplexer { get; } = connectionMultiplexer;

    /// <summary>
    /// Gets the Redis database to use
    /// </summary>
    protected StackExchange.Redis.IDatabase Database { get; } = connectionMultiplexer.GetDatabase();

    /// <summary>
    /// Gets the service used to stream input and output <see cref="CloudEvent"/>s
    /// </summary>
    protected ICloudEventBus CloudEventBus { get; } = cloudEventBus;

    /// <summary>
    /// Gets the service used to serialize/deserialize data to/from JSON
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    /// <summary>
    /// Gets the current <see cref="CorrelatorOptions"/>
    /// </summary>
    protected CorrelatorOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets the <see cref="RedisCloudEventIngestor"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    /// Gets a boolean indicating whether or not the Redis version used by the cloud event ingestor supports streaming commands
    /// </summary>
    protected bool SupportsStreaming { get; private set; }

    /// <summary>
    /// Gets the name of the cloud event consumer group the correlator belongs to
    /// </summary>
    protected string ConsumerGroup => this.Options.Events?.ConsumerGroup ?? $"{this.Options.Name}.{this.Options.Namespace}";

    /// <summary>
    /// Gets the key of the queue of the correlator's cloud event consumer group
    /// </summary>
    protected string ConsumerGroupQueue => $"cloud-event-consumer-group:{this.ConsumerGroup}";

    /// <summary>
    /// Gets the name of the correlator's cloud event consumer
    /// </summary>
    protected string Consumer => $"{this.Options.Name}.{this.Options.Namespace}";

    /// <summary>
    /// Gets the name of the queue used to push messages being processed by the consumer
    /// </summary>
    protected string ConsumerProcessingQueue => $"{this.Consumer}@{this.ConsumerGroup}:processing";

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        var version = ((string)(await this.Database.ExecuteAsync("INFO", "server").ConfigureAwait(false))!).Split('\n').FirstOrDefault(line => line.StartsWith("redis_version:"))?[14..]?.Trim() ?? "undetermined";
        try
        {
            if (!(await this.Database.KeyExistsAsync(SynapseDefaults.CloudEvents.Bus.StreamName).ConfigureAwait(false))
                || (await this.Database.StreamGroupInfoAsync(SynapseDefaults.CloudEvents.Bus.StreamName).ConfigureAwait(false)).All(x => x.Name != this.ConsumerGroup))
                await this.Database.StreamCreateConsumerGroupAsync(SynapseDefaults.CloudEvents.Bus.StreamName, this.ConsumerGroup, StreamPosition.NewMessages, true).ConfigureAwait(false);
            this.SupportsStreaming = true;
            this.Logger.LogInformation("Redis server version '{version}' supports streaming commands. Streaming feature is enabled", version);
        }
        catch (RedisServerException ex) when (ex.Message.StartsWith("ERR unknown command"))
        {
            if (!(await this.Database.KeyExistsAsync(SynapseDefaults.CloudEvents.Bus.StreamName).ConfigureAwait(false))
                || !(await this.Database.SetContainsAsync(SynapseDefaults.CloudEvents.Bus.ConsumerGroupListKey, this.ConsumerGroupQueue).ConfigureAwait(false)))
                await this.Database.SetAddAsync(SynapseDefaults.CloudEvents.Bus.ConsumerGroupListKey, this.ConsumerGroupQueue).ConfigureAwait(false);
            this.SupportsStreaming = false;
            this.Logger.LogInformation("Redis server version '{version}' does not support streaming commands. Streaming feature is emulated using lists", version);
        }
        _ = this.StreamCloudEventsAsync();
    }

    /// <summary>
    /// Ingests <see cref="CloudEvent"/>s published on the dedicated Redis stream
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task StreamCloudEventsAsync()
    {
        try
        {
            if (this.SupportsStreaming)
            {
                var pendingInfo = await this.Database.StreamPendingAsync(SynapseDefaults.CloudEvents.Bus.StreamName, this.ConsumerGroup).ConfigureAwait(false);
                var consumerInfo = pendingInfo.Consumers.FirstOrDefault(c => c.Name == this.Consumer);
                if (consumerInfo.PendingMessageCount > 0)
                {
                    var pendingMessages = await this.Database.StreamReadGroupAsync(SynapseDefaults.CloudEvents.Bus.StreamName, this.ConsumerGroup, this.Consumer, count: consumerInfo.PendingMessageCount).ConfigureAwait(false);
                    if (pendingMessages != null && pendingMessages.Length > 0) foreach (var message in pendingMessages) await this.ExtractAndPublishCloudEventAsync(message).ConfigureAwait(false);
                }
                while (!this.CancellationTokenSource.IsCancellationRequested)
                {
                    var pendingMessages = await this.Database.StreamReadGroupAsync(SynapseDefaults.CloudEvents.Bus.StreamName, this.ConsumerGroup, this.Consumer, ">", 1).ConfigureAwait(false);
                    if (pendingMessages != null && pendingMessages.Length > 0) await this.ExtractAndPublishCloudEventAsync(pendingMessages.First()).ConfigureAwait(false);
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
            else
            {
                var pendingMessages = await this.Database.ListRangeAsync(this.ConsumerProcessingQueue, 0, -1).ConfigureAwait(false);
                if (pendingMessages != null && pendingMessages.Length > 0) foreach (var message in pendingMessages) await this.ExtractAndPublishCloudEventAsync(message).ConfigureAwait(false);
                while (!this.CancellationTokenSource.IsCancellationRequested)
                {
                    var message = await this.Database.ListRightPopLeftPushAsync(this.ConsumerGroupQueue, this.ConsumerProcessingQueue).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(message)) await this.ExtractAndPublishCloudEventAsync(message).ConfigureAwait(false);
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while ingesting cloud events: {ex}", ex);
        }
    }

    /// <summary>
    /// Extracts a <see cref="CloudEvent"/> from the specified <see cref="StreamEntry"/> and publish it onto the correlator's <see cref="ICloudEventBus"/>
    /// </summary>
    /// <param name="message">The <see cref="StreamEntry"/> to extract the <see cref="CloudEvent"/> from</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ExtractAndPublishCloudEventAsync(StreamEntry message)
    {
        try
        {
            var json = (string)message[SynapseDefaults.CloudEvents.Bus.EventFieldName]!;
            var e = this.JsonSerializer.Deserialize<CloudEvent>(json)!;
            this.CloudEventBus.InputStream.OnNext(e);
            await this.Database.StreamAcknowledgeAsync(SynapseDefaults.CloudEvents.Bus.StreamName, this.ConsumerGroup, message.Id).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occurred while extracting the cloud event from the message with id '{messageId}': {ex}", message.Id, ex);
        }
    }

    /// <summary>
    /// Extracts a <see cref="CloudEvent"/> from the specified <see cref="StreamEntry"/> and publish it onto the correlator's <see cref="ICloudEventBus"/>
    /// </summary>
    /// <param name="message">The <see cref="RedisValue"/> to extract the <see cref="CloudEvent"/> from</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task ExtractAndPublishCloudEventAsync(RedisValue message)
    {
        try
        {
            var e = this.JsonSerializer.Deserialize<CloudEvent>(message!)!;
            this.CloudEventBus.InputStream.OnNext(e);
            await this.Database.ListLeftPopAsync(this.ConsumerProcessingQueue).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while extracting the cloud event from the specified message: {ex}", ex);
        }
    }

    /// <inheritdoc/>
    public virtual Task StopAsync(CancellationToken cancellationToken) => this.CancellationTokenSource.CancelAsync();

}
