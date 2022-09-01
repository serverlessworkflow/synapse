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

            var x = bool.TryParse(env, out _);

            if (!string.IsNullOrWhiteSpace(env)
                && bool.TryParse(env, out var skipCertificateValidation))
                this.SkipCertificateValidation = skipCertificateValidation;
        }

        /// <summary>
        /// Gets/sets the options used to configure the way Synapse archives <see cref="V1WorkflowInstance"/>s
        /// </summary>
        public virtual ArchivingOptions Archiving { get; set; } = new();

        /// <summary>
        /// Gets/sets the options used to configure the application's cloud eventss
        /// </summary>
        public virtual CloudEventOptions CloudEvents { get; set; } = new();

        /// <summary>
        /// Gets/sets the options sued to configure the application's persistence layer
        /// </summary>
        public virtual PersistenceOptions Persistence { get; set; } = new();

        /// <summary>
        /// Gets/sets the options sued to configure the application's plugins directory
        /// </summary>
        public virtual PluginsOptions Plugins { get; set; } = new();

        /// <summary>
        /// Gets/sets the path to the directory used to monitor workflow definition files
        /// </summary>
        public virtual string DefinitionsDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "data", "definitions");

        /// <summary>
        /// Gets/sets a boolean indicating whether or not to skip certificate validation when performing http requests
        /// </summary>
        public virtual bool SkipCertificateValidation { get; set; }

    }

}
