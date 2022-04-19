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

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IPluginManager"/> interface
    /// </summary>
    public class PluginManager
        : BackgroundService, IPluginManager
    {

        /// <summary>
        /// Initializes a new <see cref="PluginManager"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="applicationOptions">The service used to access the current <see cref="SynapseApplicationOptions"/></param>
        public PluginManager(ILogger<PluginManager> logger, IOptions<SynapseApplicationOptions> applicationOptions)
        {
            this.Logger = logger;
            this.ApplicationOptions = applicationOptions.Value;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Gets the service used to watch plugin files
        /// </summary>
        protected FileSystemWatcher FileSystemWatcher { get; private set; } = null!;

        /// <summary>
        /// Gets a <see cref="SynchronizedCollection{T}"/> containing all loaded <see cref="IPlugin"/>s
        /// </summary>
        protected SynchronizedCollection<IPlugin> Plugins { get; } = new();

        IEnumerable<IPlugin> IPluginManager.Plugins => this.Plugins;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var directory = new DirectoryInfo(this.ApplicationOptions.PluginsDirectory);
            if (!directory.Exists)
                directory.Create();
            this.Logger.LogInformation("Loading application plugins...");
            foreach (var path in directory.GetFiles("*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    this.LoadPluginFrom(path.FullName);
                }
                catch(Exception ex)
                {
                    this.Logger.LogWarning("An error occured while loading the plugin '{pluginFilePath}': {ex}", path, ex.ToString());
                }
            }
            this.Logger.LogInformation("Application plugins loaded");
            this.FileSystemWatcher = new(directory.FullName, "*.dll");
            this.FileSystemWatcher.NotifyFilter = NotifyFilters.Attributes
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.Security
                | NotifyFilters.Size;
            this.FileSystemWatcher.IncludeSubdirectories = true;
            this.FileSystemWatcher.EnableRaisingEvents = true;
            this.FileSystemWatcher.Changed += this.OnPluginFileDetected;
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual IPlugin LoadPluginFrom(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            var file = new FileInfo(path);
            if (!file.Exists)
                throw new FileNotFoundException("Failed to find the specified plugin file", path);
            var loadContext = new PluginAssemblyLoadContext(path);
            var assembly = loadContext.LoadFromAssemblyName(new(Path.GetFileNameWithoutExtension(file.FullName)));
            var plugin = new Plugin(loadContext, assembly);
            this.Plugins.Add(plugin);
            return plugin;
        }

        /// <summary>
        /// Handles the detection of a new plugin file
        /// </summary>
        /// <param name="sender">The <see cref="System.IO.FileSystemWatcher"/> that has detected the plugin file</param>
        /// <param name="e">An object that describes the detected plugin file</param>
        protected virtual void OnPluginFileDetected(object sender, FileSystemEventArgs e)
        {
            if (this.Plugins.ToList().Any(p => p.Assembly.Location == e.FullPath))
                return;
            this.LoadPluginFrom(e.FullPath);
        }

    }

}
