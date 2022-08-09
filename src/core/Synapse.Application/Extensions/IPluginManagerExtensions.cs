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

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="IPluginManager"/>s
    /// </summary>
    public static class IPluginManagerExtensions
    {

        /// <summary>
        /// Gets the <see cref="IPlugin"/> of the specified type
        /// </summary>
        /// <typeparam name="TPlugin">The type of the <see cref="IPlugin"/> to get</typeparam>
        /// <param name="pluginManager">The extended <see cref="IPluginManager"/></param>
        /// <returns>The <see cref="IPlugin"/> of the specified type</returns>
        public static TPlugin GetRequiredPlugin<TPlugin>(this IPluginManager pluginManager)
            where TPlugin : IPlugin
        {
            var plugin = pluginManager.GetPlugin<TPlugin>();
            if (plugin == null)
                throw new NullReferenceException($"Failed to find the required plugin of type '{typeof(TPlugin).FullName}'");
            return plugin;
        }

        /// <summary>
        /// Gets the <see cref="IPlugin"/> of the specified type, with the specified name
        /// </summary>
        /// <typeparam name="TPlugin">The type of the <see cref="IPlugin"/> to get</typeparam>
        /// <param name="pluginManager">The extended <see cref="IPluginManager"/></param>
        /// <param name="name">The name of the <see cref="IPlugin"/> to get</param>
        /// <returns>The <see cref="IPlugin"/> of the specified type</returns>
        public static TPlugin? GetPlugin<TPlugin>(this IPluginManager pluginManager, string name)
            where TPlugin : IPlugin
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return (TPlugin)pluginManager.GetPlugin(name)!;
        }

        /// <summary>
        /// Attempts to get the <see cref="IPlugin"/> with the specified name
        /// </summary>
        /// <param name="pluginManager">The extended <see cref="IPluginManager"/></param>
        /// <param name="name">The name of the <see cref="IPlugin"/> to get</param>
        /// <param name="plugin">The matched plugin</param>
        /// <returns>A boolean indicating whether or an <see cref="IPlugin"/> with the specified name could be found</returns>
        public static bool TryGetPlugin(this IPluginManager pluginManager, string name, out IPlugin plugin)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            plugin = pluginManager.GetPlugin(name)!;
            return plugin != null;
        }

        /// <summary>
        /// Attempts to get the <see cref="IPlugin"/> with the specified name
        /// </summary>
        /// <typeparam name="TPlugin">The type of the <see cref="IPlugin"/> to get</typeparam>
        /// <param name="pluginManager">The extended <see cref="IPluginManager"/></param>
        /// <param name="name">The name of the <see cref="IPlugin"/> to get</param>
        /// <param name="plugin">The matched plugin</param>
        /// <returns>A boolean indicating whether or an <see cref="IPlugin"/> with the specified name could be found</returns>
        public static bool TryGetPlugin<TPlugin>(this IPluginManager pluginManager, string name, out TPlugin plugin)
            where TPlugin : IPlugin
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            plugin = pluginManager.GetPlugin<TPlugin>(name)!;
            return plugin != null;
        }

    }

}
