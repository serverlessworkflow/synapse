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
        /// Initializes a new <see cref="SynapseApplicationOptions"/>
        /// </summary>
        public SynapseApplicationOptions()
        {
            var env = EnvironmentVariables.SkipCertificateValidation.Value;
            if (!string.IsNullOrWhiteSpace(env)
                && bool.TryParse(env, out var skipCertificateValidation))
                this.SkipCertificateValidation = skipCertificateValidation;
        }

        /// <summary>
        /// Gets/sets a boolean indicating whether or not to skip certificate validation when performing http requests
        /// </summary>
        public virtual bool SkipCertificateValidation { get; set; }

        /// <summary>
        /// Gets/sets the options used to configure the application's cloud eventss
        /// </summary>
        public virtual CloudEventOptions CloudEvents { get; set; } = new();

    }

}
