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

namespace Synapse.Application.Configuration
{

    /// <summary>
    /// Represents the options used to configure the Synapse application
    /// </summary>
    public class SynapseApplicationOptions
    {

        /// <summary>
        /// Gets/sets the options used to configure the application's cloud eventss
        /// </summary>
        public virtual CloudEventOptions CloudEvents { get; set; } = new();

    }

    /// <summary>
    /// Publishes the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="e">The <see cref="CloudEvent"/> to publish</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public delegate Task PublishIntegrationEventDelegate(IServiceProvider serviceProvider, CloudEvent e, CancellationToken cancellationToken);

}
