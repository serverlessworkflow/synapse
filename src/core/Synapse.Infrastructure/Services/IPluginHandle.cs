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
    /// Defines the fundamentals of an object used to handle a plugin
    /// </summary>
    public interface IPluginHandle
        : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Gets the event fired whenever the <see cref="IPluginHandle"/> has been disposed of
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Gets the path to the handled <see cref="IPlugin"/>'s metadata file
        /// </summary>
        string MetadataFilePath { get; }

        /// <summary>
        /// Gets a boolean indicating whether or not the <see cref="IPlugin"/> is loaded
        /// </summary>
        bool IsLoaded{ get; }

        /// <summary>
        /// Gets an object used to describe the handled <see cref="IPlugin"/>
        /// </summary>
        PluginMetadata Metadata { get; }

        /// <summary>
        /// Loads and initializes the <see cref="IPlugin"/>
        /// </summary>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> used to manage the handled <see cref="IPlugin"/>'s lifetime</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        ValueTask LoadAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Gets the <see cref="IPlugin"/>
        /// </summary>
        /// <remarks>Will throw a new <see cref="AppDomainUnloadedException"/> if the <see cref="IPluginHandle"/> is not loaded</remarks>
        /// <returns>The loaded <see cref="IPlugin"/></returns>
        IPlugin GetPlugin();

        /// <summary>
        /// Unloads the <see cref="IPlugin"/>
        /// </summary>
        void Unload();

    }

}
