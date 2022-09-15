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

namespace Synapse.Integration.Models
{

    /// <summary>
    /// Describe a plugin
    /// </summary>
    public class V1PluginInfo
    {

        /// <summary>
        /// Initializes a new <see cref="V1PluginInfo"/>
        /// </summary>
        protected V1PluginInfo()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1PluginInfo"/>
        /// </summary>
        /// <param name="location">The plugin's location/param>
        /// <param name="metadata">The plugin's metadata</param>
        /// <param name="isLoaded">A boolean indicating whether or not the described plugin is loaded</param>
        public V1PluginInfo(string location, V1PluginMetadata metadata, bool isLoaded)
        {
            this.Location = location;
            this.Metadata = metadata;
            this.IsLoaded = isLoaded;
        }

        /// <summary>
        /// Gets the name of the plugin's location
        /// </summary>
        public virtual string Location { get; }

        /// <summary>
        /// Gets the plugin's metadata
        /// </summary>
        public virtual V1PluginMetadata Metadata { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the described plugin is loaded
        /// </summary>
        public virtual bool IsLoaded { get; set; }

    }

}
