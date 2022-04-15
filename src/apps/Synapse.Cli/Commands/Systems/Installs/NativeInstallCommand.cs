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

using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Synapse.Cli.Commands.Systems.Installs
{

    /// <summary>
    /// Represents the <see cref="Command"/> used to perform a native Synapse install
    /// </summary>
    internal class NativeInstallCommand
        : Command
    {

        /// <summary>
        /// Gets the <see cref="InstallCommand"/>'s name
        /// </summary>
        public const string CommandName = "native";
        /// <summary>
        /// Gets the <see cref="InstallCommand"/>'s description
        /// </summary>
        public const string CommandDescription = "Installs Synapse on the current OS";

        /// <inheritdoc/>
        public NativeInstallCommand(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ISynapseManagementApi synapseManagementApi, IHttpClientFactory httpClientFactory)
            : base(serviceProvider, loggerFactory, synapseManagementApi, CommandName, CommandDescription)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.AddOption(CommandOptions.Directory);
            this.Handler = CommandHandler.Create<string>(this.HandleAsync);
        }

        /// <summary>
        /// Gets the service used to perform HTTP requests
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Handles the <see cref="NativeInstallCommand"/>
        /// </summary>
        /// <param name="directory">The directory to install Synapse to</param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public async Task HandleAsync(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CNCF", "Synapse");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    directory = Path.Combine("usr", "local", "cncf", "synapse");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    directory = Path.Combine("Applications", "CNCF", "Synapse");
                else
                    throw new PlatformNotSupportedException();
            }
            var directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            var target = null as string;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                target = "win-x64.zip";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                target = "linux-x64.tar.gz";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                target = "osx-64.tar.gz";
            else
                throw new PlatformNotSupportedException();
            using var packageStream = new MemoryStream();
            await AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn(),
                })
                .HideCompleted(true)
                .StartAsync(async context =>
                {
                    await Task.Run(async () =>
                    {
                        var task = context.AddTask($"Downloading [u]synapse-{target}[/]", new ProgressTaskSettings
                        {
                            AutoStart = false
                        });
                        await this.HttpClient.DownloadAsync($"https://github.com/serverlessworkflow/synapse/releases/download/0.1.0/synapse-{target}", packageStream, task);
                    });
                });
            AnsiConsole.Status()
                .Start("Extracting [u]synapse-{target}[/]...", ctx =>
                {
                    using var archive = new ZipArchive(packageStream, ZipArchiveMode.Read);
                    archive.ExtractToDirectory(directoryInfo.FullName, true);
                });
        }

        private static class CommandOptions
        {

            public static Option<bool> Directory
            {
                get
                {
                    var option = new Option<bool>("--directory")
                    {
                        Description = "The directory to install Synapse to"
                    };
                    option.AddAlias("-d");
                    return option;
                }
            }

        }

    }

}
