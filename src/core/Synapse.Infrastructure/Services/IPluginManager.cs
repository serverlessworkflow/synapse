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

namespace Synapse.Infrastructure.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to manage plugins
    /// </summary>
    public interface IPluginManager
    {

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing all detected <see cref="IPlugin"/>s
        /// </summary>
        IEnumerable<IPluginHandle> Plugins { get; }

        /// <summary>
        /// Loads the specified <see cref="IPlugin"/>
        /// </summary>
        /// <param name="pluginHandle">The service used to handle the <see cref="IPlugin"/> to load</param>
        /// <returns>The loaded <see cref="IPlugin"/></returns>
        Task<IPlugin> LoadPluginAsync(IPluginHandle pluginHandle);

        /// <summary>
        /// Unloads the specified <see cref="IPlugin"/>
        /// </summary>
        /// <param name="pluginHandle">The service used to handle the <see cref="IPlugin"/> to unload</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        ValueTask UnloadPluginAsync(IPluginHandle pluginHandle);

        /// <summary>
        /// Gets the <see cref="IPlugin"/> of the specified type, with the specified name
        /// </summary>
        /// <param name="name">The name of the <see cref="IPlugin"/> to get</param>
        /// <returns>The <see cref="IPlugin"/> of the specified type</returns>
        IPlugin? GetPlugin(string name);

        /// <summary>
        /// Gets the <see cref="IPlugin"/> of the specified type
        /// </summary>
        /// <typeparam name="TPlugin">The type of the <see cref="IPlugin"/> to get</typeparam>
        /// <returns>The <see cref="IPlugin"/> of the specified type</returns>
        TPlugin? GetPlugin<TPlugin>()
            where TPlugin : IPlugin;

        /// <summary>
        /// Gets all <see cref="IPlugin"/>s of the specified type
        /// </summary>
        /// <typeparam name="TPlugin">The type of the <see cref="IPlugin"/>s to get</typeparam>
        /// <returns>The <see cref="IPlugin"/> of the specified type</returns>
        IEnumerable<TPlugin> GetPlugins<TPlugin>()
            where TPlugin : IPlugin;

    }

}
