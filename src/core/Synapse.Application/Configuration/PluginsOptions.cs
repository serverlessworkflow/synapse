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
    /// Represents the options used to configure the application's plugins
    /// </summary>
    public class PluginsOptions
    {

        /// <summary>
        /// Initializes a new <see cref="PluginsOptions"/>
        /// </summary>
        public PluginsOptions()
        {
            var env = EnvironmentVariables.Plugins.Directory.Value;
            if (!string.IsNullOrWhiteSpace(env))
                this.Directory = env;
        }

        /// <summary>
        /// Gets/sets the path to the application's plugins directory
        /// </summary>
        public virtual string Directory { get; set; } = Path.Combine(AppContext.BaseDirectory, "plugins");

    }

}
