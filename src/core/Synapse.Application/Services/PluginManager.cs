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

using Neuroglia.Serialization;
using Synapse.Infrastructure.Plugins;
using System.IO.Compression;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IPluginManager"/> interface
    /// </summary>
    public class PluginManager
        : BackgroundService, IPluginManager
    {

        /// <summary>
        /// Gets the name of an <see cref="IPlugin"/> metadata file
        /// </summary>
        public const string PluginMetadataFileName = "plugin.json";

        /// <summary>
        /// Initializes a new <see cref="PluginManager"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="jsonSerializer">The service to serialize/deserialize to/from JSON</param>
        /// <param name="applicationOptions">The service used to access the current <see cref="SynapseApplicationOptions"/></param>
        public PluginManager(IServiceProvider serviceProvider, ILogger<PluginManager> logger, IJsonSerializer jsonSerializer, IOptions<SynapseApplicationOptions> applicationOptions)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.JsonSerializer = jsonSerializer;
            this.ApplicationOptions = applicationOptions.Value;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Gets the service used to watch the <see cref="IPlugin"/> files
        /// </summary>
        protected FileSystemWatcher FileSystemWatcher { get; private set; } = null!;

        /// <summary>
        /// Gets the <see cref="PluginManager"/>'s <see cref="CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; private set; } = null!;

        /// <summary>
        /// Gets an <see cref="SynchronizedCollection{T}"/> containing the handles to all discovered <see cref="IPlugin"/>s
        /// </summary>
        public SynchronizedCollection<IPluginHandle> Plugins { get; } = new();

        IEnumerable<IPluginHandle> IPluginManager.Plugins => this.Plugins;

        /// <summary>
        /// Gets the <see cref="TaskCompletionSource"/> used to wait for the <see cref="PluginManager"/>'s 
        /// </summary>
        protected TaskCompletionSource StartupTaskCompletionSource { get; } = new();

        /// <inheritdoc/>
        public async ValueTask WaitForStartupAsync(CancellationToken stoppingToken)
        {
            await this.StartupTaskCompletionSource.Task;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var pluginDirectory = new DirectoryInfo(this.ApplicationOptions.Plugins.Directory);
            if(!pluginDirectory.Exists)
                pluginDirectory.Create();
            foreach(var packageFile in pluginDirectory.GetFiles("*.tar.gz"))
            {
                await TarGzPackage.ExtractToDirectoryAsync(packageFile.FullName, packageFile.Directory!.FullName, stoppingToken);
                packageFile.Delete();
            }
            var plugins = await this.FindPluginsAsync(pluginDirectory.FullName);
            foreach (var plugin in plugins)
            {
                this.Logger.LogInformation("Loading plugin '{plugin}'...", plugin.ToString());
                try
                {
                    await plugin.LoadAsync(this.CancellationTokenSource.Token);
                    this.Logger.LogInformation("The plugin '{plugin}' has been successfully loaded", plugin.ToString());
                }
                catch(Exception ex)
                {
                    this.Logger.LogWarning("An error occured while loading plugin '{plugin}': {ex}", plugin.ToString(), ex.ToString());
                    continue;
                }
            }
            this.FileSystemWatcher = new(pluginDirectory.FullName, $"*.*");
            this.FileSystemWatcher.IncludeSubdirectories = true;
            this.FileSystemWatcher.Created += this.OnPluginFileCreatedAsync;
            this.FileSystemWatcher.Deleted += this.OnPluginFileDeletedAsync;
            this.FileSystemWatcher.EnableRaisingEvents = true;
            this.StartupTaskCompletionSource.SetResult();
        }

        /// <summary>
        /// Finds all <see cref="IPlugin"/>s in the specified directory
        /// </summary>
        /// <param name="directoryPath">The path of the directory to scan for <see cref="IPlugin"/>s</param>
        /// <returns>A new <see cref="IEnumerable{T}"/> containing all <see cref="IPluginHandle"/>s that have been found</returns>
        public virtual async Task<IEnumerable<IPluginHandle>> FindPluginsAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));
            this.Logger.LogInformation("Scanning directory'{directory}' for plugins...", directoryPath);
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
                throw new DirectoryNotFoundException($"Failed to find the specified directory '{directoryPath}'");
            var pluginFiles = directory.GetFiles(PluginMetadataFileName, SearchOption.AllDirectories);
            this.Logger.LogInformation("Found {results} matching plugin files in directory '{directory}'", pluginFiles.Count(), directoryPath);
            var plugins = new List<IPluginHandle>(pluginFiles.Count());
            foreach (var pluginFile in pluginFiles)
            {
                plugins.Add(await this.FindPluginAsync(pluginFile.FullName));
            }
            this.Logger.LogInformation("{pluginCount} plugins have been found in '{directory}' directory", plugins.Count(), directoryPath);
            return plugins;
        }

        /// <summary>
        /// Finds the <see cref="IPlugin"/> at the specified file path
        /// </summary>
        /// <param name="metadataFilePath">The file path of the <see cref="IPlugin"/> to find</param>
        /// <returns>A new <see cref="IPluginHandle"/></returns>
        public virtual async Task<IPluginHandle> FindPluginAsync(string metadataFilePath)
        {
            if (string.IsNullOrWhiteSpace(metadataFilePath))
                throw new ArgumentNullException(nameof(metadataFilePath));
            var plugin = this.Plugins.FirstOrDefault(p => p.MetadataFilePath == metadataFilePath);
            if (plugin != null)
                return plugin;
            var file = new FileInfo(metadataFilePath);
            if (!file.Exists)
                throw new DirectoryNotFoundException($"Failed to find the specified file '{metadataFilePath}'");
            if (file.Name != PluginMetadataFileName)
                throw new Exception($"The specified file '{metadataFilePath}' is not a valid plugin metadata file");
            using var fileStream = file.OpenRead();
            var pluginMetadata = await this.JsonSerializer.DeserializeAsync<PluginMetadata>(fileStream, this.CancellationTokenSource.Token);
            plugin = ActivatorUtilities.CreateInstance<PluginHandle>(this.ServiceProvider, pluginMetadata, metadataFilePath);
            plugin.Disposed += (sender, e) => this.OnPluginHandleDisposed((IPluginHandle)sender!);
            this.Plugins.Add(plugin);
            return plugin;
        }

        /// <inheritdoc/>
        public virtual async Task<IPlugin> LoadPluginAsync(IPluginHandle pluginHandle)
        {
            if(pluginHandle == null)
                throw new ArgumentNullException(nameof(pluginHandle));
            await pluginHandle.LoadAsync(this.CancellationTokenSource.Token);
            return pluginHandle.GetPlugin();
        }

        /// <inheritdoc/>
        public virtual async ValueTask UnloadPluginAsync(IPluginHandle pluginHandle)
        {
            if (pluginHandle == null)
                throw new ArgumentNullException(nameof(pluginHandle));
            pluginHandle.Unload();
            await ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual IPlugin? GetPlugin(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            var pluginHandle = this.Plugins.FirstOrDefault(p => p.Metadata.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return pluginHandle?.GetPlugin();
        }

        /// <inheritdoc/>
        public virtual TPlugin? GetPlugin<TPlugin>()
            where TPlugin : IPlugin
        {
            return this.Plugins.OfType<TPlugin>()
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TPlugin> GetPlugins<TPlugin>()
            where TPlugin : IPlugin
        {
            return this.Plugins.OfType<TPlugin>()
                .ToList();
        }

        /// <summary>
        /// Handles the creation of a new <see cref="IPlugin"/> file
        /// </summary>
        /// <param name="sender">The service used to watch the <see cref="IPlugin"/> files</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> to handle</param>
        protected virtual async void OnPluginFileCreatedAsync(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.EndsWith(".tar.gz"))
            {
                var packageFile = new FileInfo(e.FullPath);
                do
                {
                    await Task.Delay(250);
                }
                while (packageFile.IsLocked());
                using var packageFileStream = packageFile.OpenRead();
                await TarGzPackage.ExtractToDirectoryAsync(packageFile.FullName, packageFile.Directory!.FullName);
                await packageFileStream.DisposeAsync();
                await this.FindPluginsAsync(packageFile.Directory!.FullName);
                packageFile.Delete();
            }
            else if(e.FullPath == "plugin.json")
            {
                await this.FindPluginAsync(e.FullPath);
            }
        }

        /// <summary>
        /// Handles the deletion of a new <see cref="IPlugin"/> file
        /// </summary>
        /// <param name="sender">The service used to watch the <see cref="IPlugin"/> files</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> to handle</param>
        protected virtual async void OnPluginFileDeletedAsync(object sender, FileSystemEventArgs e)
        {
            var pluginHandler = this.Plugins.FirstOrDefault(p => p.MetadataFilePath == e.FullPath);
            if (pluginHandler != null)
                pluginHandler.Dispose();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles the disposal of the specified <see cref="IPluginHandle"/>
        /// </summary>
        /// <param name="pluginHandle">The <see cref="IPluginHandle"/> that has been disposed of</param>
        protected virtual void OnPluginHandleDisposed(IPluginHandle pluginHandle)
        {
            this.Plugins.Remove(pluginHandle);
        }

    }

}
