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
    /// Represents the options used to configure the application's cloud eventss
    /// </summary>
    public class CloudEventOptions
    {

        /// <summary>
        /// Initializes a new <see cref="CloudEventOptions"/>
        /// </summary>
        public CloudEventOptions()
        {
            string? source = EnvironmentVariables.CloudEvents.Source.Value;
            if (!string.IsNullOrWhiteSpace(source))
                this.Source = new(source);
        }

        /// <summary>
        /// Gets/sets the value of the <see cref="CloudEvent.Source"/> property for all <see cref="CloudEvent"/>s produced by the application
        /// </summary>
        public virtual Uri Source { get; set; } = new("https://synapse.io/runtime/events");

        /// <summary>
        /// Gets/sets the options used to configure the broker to post cloud events to
        /// </summary>
        public virtual CloudEventBrokerOptions Broker { get; set; } = new();

    }

}
