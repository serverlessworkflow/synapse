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

using Synapse.Application.Commands.Correlations;
using System.Reactive.Subjects;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents a service used to correlate <see cref="CloudEvent"/>s
    /// </summary>
    public class CloudEventCorrelator
        : BackgroundService
    {

        /// <summary>
        /// Initializes a new <see cref="CloudEventCorrelator"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="cloudEventStream">The <see cref="ISubject{T}"/> used to stream <see cref="CloudEvent"/>s</param>
        public CloudEventCorrelator(IServiceProvider serviceProvider , ILogger<CloudEventCorrelator> logger, ISubject<CloudEvent> cloudEventStream)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.CloudEventStream = cloudEventStream;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="ISubject{T}"/> used to stream <see cref="CloudEvent"/>s
        /// </summary>
        protected ISubject<CloudEvent> CloudEventStream { get; }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.CloudEventStream.SubscribeAsync(async e =>
            {
                using var scope = this.ServiceProvider.CreateScope();
                await this.CorrelateAsync(scope.ServiceProvider, e, stoppingToken);
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ingests the specified <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="e">The <see cref="CloudEvent"/> to ingest</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task CorrelateAsync(IServiceProvider serviceProvider, CloudEvent e, CancellationToken cancellationToken)
        {
            try
            {
                var mediator = serviceProvider.GetRequiredService<IMediator>();
                await mediator.ExecuteAndUnwrapAsync(new V1CorrelateEventCommand(V1Event.CreateFrom(e)), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("An error occured while processing an incoming cloud event: {ex}", ex.ToString());
            }
        }

    }

}
