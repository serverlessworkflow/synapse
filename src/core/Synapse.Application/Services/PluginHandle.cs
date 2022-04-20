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
using System.Reflection;
using System.Runtime.Loader;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default <see cref="IPluginHandle"/> implementation
    /// </summary>
    public class PluginHandle
        : IPluginHandle
    {

        /// <inheritdoc/>
        public event EventHandler? Disposed;

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="PluginHandle"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="metadata">An object used to describe the handled <see cref="IPlugin"/></param>
        /// <param name="metadataFilePath">The path to the handled <see cref="IPlugin"/> <see cref="System.Reflection.Assembly"/> file</param>
        public PluginHandle(IServiceProvider serviceProvider, PluginMetadata metadata, string metadataFilePath)
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.MetadataFilePath = metadataFilePath ?? throw new ArgumentNullException(nameof(metadataFilePath));
            this.AssemblyFilePath = Path.Combine(Path.GetDirectoryName(this.MetadataFilePath)!, this.Metadata.AssemblyFileName);
            if (!File.Exists(metadataFilePath))
                throw new FileNotFoundException(nameof(metadataFilePath));
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public virtual PluginMetadata Metadata { get; }

        /// <inheritdoc/>
        public virtual bool IsLoaded { get; protected set; }

        /// <summary>
        /// Gets the path to the <see cref="IPlugin"/>'s <see cref="PluginMetadata"/> file
        /// </summary>
        public virtual string MetadataFilePath { get; }

        /// <summary>
        /// Gets the path to the <see cref="IPlugin"/>'s <see cref="System.Reflection.Assembly"/> file
        /// </summary>
        protected virtual string AssemblyFilePath { get; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s <see cref="AssemblyLoadContext"/>
        /// </summary>
        protected AssemblyLoadContext? LoadContext { get; set; }

        /// <summary>
        /// Gets the <see cref="IPlugin"/>'s <see cref="System.Reflection.Assembly"/>
        /// </summary>
        protected Assembly? Assembly { get; set; }

        /// <summary>
        /// Gets the loaded <see cref="IPlugin"/>, if <see cref="IsLoaded"/>
        /// </summary>
        protected IPlugin? Plugin { get; set; }

        /// <summary>
        /// Gets the <see cref="PluginHandle"/>'s <see cref="System.Threading.CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource? CancellationTokenSource { get; private set; }

        /// <inheritdoc/>
        public virtual async ValueTask LoadAsync(CancellationToken stoppingToken)
        {
            if (this.IsLoaded)
                return;
            try
            {
                this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                this.LoadContext = new PluginAssemblyLoadContext(Path.GetDirectoryName(this.AssemblyFilePath)!);
                this.Assembly = this.LoadContext.LoadFromAssemblyName(new(Path.GetFileNameWithoutExtension(this.AssemblyFilePath)));
                var pluginType = this.Assembly.GetTypes().FirstOrDefault(t => t.IsClass && !t.IsInterface && !t.IsGenericTypeDefinition && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t));
                if (pluginType == null)
                    throw new TypeLoadException($"Failed to find a valid plugin type in the specified assembly '{this.Assembly.FullName}'");
                this.Plugin = (IPlugin)ActivatorUtilities.CreateInstance(this.ServiceProvider, pluginType);
                await this.Plugin.InitializeAsync(this.CancellationTokenSource.Token);
                this.IsLoaded = true;
                await ValueTask.CompletedTask;
            }
            catch
            {
                this.Unload();
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual IPlugin GetPlugin()
        {
            if (!this.IsLoaded)
                throw new AppDomainUnloadedException();
            return this.Plugin!;
        }

        /// <inheritdoc/>
        public virtual void Unload()
        {
            if (!this.IsLoaded)
                return;
            this.Assembly = null;
            this.LoadContext?.Unload();
            this.LoadContext = null;
            this.Plugin?.Dispose();
            this.Plugin = null;
            this.IsLoaded = true;
        }

        /// <summary>
        /// Disposes of the <see cref="IPluginManager"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="IPluginManager"/> is being disposed of</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Unload();
                }
                this._Disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsync(true);
            this.Disposed?.Invoke(this, new());
            this.CancellationTokenSource?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the <see cref="IPluginManager"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="IPluginManager"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.Unload();
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            this.Disposed?.Invoke(this, new());
            this.CancellationTokenSource?.Dispose();
            GC.SuppressFinalize(this);
        }
    
    }

}
