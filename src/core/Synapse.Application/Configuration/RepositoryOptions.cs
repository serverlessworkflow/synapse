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

using Synapse.Infrastructure.Plugins;

namespace Synapse.Application.Configuration
{
    /// <summary>
    /// Represents the options used to configure an <see cref="IRepository"/>
    /// </summary>
    public class RepositoryOptions
    {

        /// <summary>
        /// Gets/sets the name of the <see cref="IRepositoryPlugin"/> to use
        /// </summary>
        public virtual string PluginName { get; set; } = null!;

    }

}
