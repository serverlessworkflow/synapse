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

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IIntegrationEventBus"/> interface
    /// </summary>
    public class IntegrationEventBusPipeline
        : IIntegrationEventBus
    {

        /// <summary>
        /// Initializes a new <see cref="IntegrationEventBusPipeline"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="options">The current <see cref="IntegrationEventBusPipelineOptions"/></param>
        public IntegrationEventBusPipeline(IServiceProvider serviceProvider, IOptions<IntegrationEventBusPipelineOptions> options)
        {
            this.ServiceProvider = serviceProvider;
            this.Options = options.Value;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the current <see cref="IntegrationEventBusPipelineOptions"/>
        /// </summary>
        protected IntegrationEventBusPipelineOptions Options { get; }

        /// <inheritdoc/>
        public virtual async Task PublishAsync(CloudEvent e, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();
            foreach (var middleware in this.Options.Middlewares)
            {
                tasks.Add(middleware.Invoke(this.ServiceProvider, e, cancellationToken));
            }
            await Task.WhenAll(tasks);
        }

    }

}
