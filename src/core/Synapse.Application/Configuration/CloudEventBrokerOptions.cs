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
    /// Represents the options used to configure the broker to post cloud eventss to
    /// </summary>
    public class CloudEventBrokerOptions
    {

        /// <summary>
        /// Initializes a new <see cref="CloudEventBrokerOptions"/>
        /// </summary>
        public CloudEventBrokerOptions()
        {
            string? uri = EnvironmentVariables.CloudEvents.Broker.Uri.Value;
            if (string.IsNullOrWhiteSpace(uri))
                this.Uri = null!;
            else
                this.Uri = new(uri);
        }

        /// <summary>
        /// Gets/sets the cloud event broker uri
        /// </summary>
        public virtual Uri Uri { get; set; }

    }

}
