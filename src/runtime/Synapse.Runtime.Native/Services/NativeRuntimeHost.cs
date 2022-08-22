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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Synapse.Application.Configuration;
using Synapse.Application.Services;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Services;
using Synapse.Runtime.Docker.Configuration;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the native implementation of the <see cref="IWorkflowRuntime"/>
    /// </summary>
    public class NativeRuntimeHost
        : WorkflowRuntimeHostBase
    {

        /// <summary>
        /// Initializes a new <see cref="NativeRuntimeHost"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        /// <param name="applicationOptions">The service used to access the current <see cref="SynapseApplicationOptions"/></param>
        /// <param name="options">The service used to access the current <see cref="NativeRuntimeOptions"/></param>
        public NativeRuntimeHost(ILoggerFactory loggerFactory, IHostEnvironment environment, IHttpClientFactory httpClientFactory, 
            IOptions<SynapseApplicationOptions> applicationOptions, IOptions<NativeRuntimeOptions> options)
            : base(loggerFactory)
        {
            this.Environment = environment;
            this.HttpClient = httpClientFactory.CreateClient();
            this.ApplicationOptions = applicationOptions.Value;
            this.Options = options.Value;
        }

        /// <summary>
        /// Gets the current <see cref="IHostEnvironment"/>
        /// </summary>
        protected IHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used to perform HTTP requests
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the current <see cref="SynapseApplicationOptions"/>
        /// </summary>
        protected SynapseApplicationOptions ApplicationOptions { get; }

        /// <summary>
        /// Gets the current <see cref="NativeRuntimeOptions"/>
        /// </summary>
        protected NativeRuntimeOptions Options { get; }

        /// <summary>
        /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all known worker processes
        /// </summary>
        protected ConcurrentDictionary<string, Process> Processes { get; } = new();

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.InstallWorkerAsync(stoppingToken);
        }

        /// <summary>
        /// Downloads and installs the worker binaries, if not already present
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task InstallWorkerAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogInformation("Downloading worker app...");
            var workerDirectory = new DirectoryInfo(this.Options.WorkingDirectory);
            if (!workerDirectory.Exists)
                workerDirectory.Create();
            if (File.Exists(this.Options.GetWorkerFileName()))
            {
                this.Logger.LogInformation("Worker app already present locally. Skipping download."); //todo: config based: the user might want to get latest every time
                return;
            }
            string? target;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                target = "win-x64.zip";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                target = "linux-x64.tar.gz";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                target = "osx-x64.tar.gz";
            else
                throw new PlatformNotSupportedException();
            using var packageStream = await this.HttpClient.GetStreamAsync($"https://github.com/serverlessworkflow/synapse/releases/download/{typeof(NativeRuntimeHost).Assembly.GetName().Version!.ToString(3)!}/synapse-worker-{target}", cancellationToken); //todo: config based
            using ZipArchive archive = new(packageStream, ZipArchiveMode.Read);
            this.Logger.LogInformation("Worker app successfully downloaded. Extracting...");
            archive.ExtractToDirectory(workerDirectory.FullName, true);
            this.Logger.LogInformation("Worker app successfully extracted");
        }

        /// <inheritdoc/>
        public override Task<IWorkflowProcess> CreateProcessAsync(V1Workflow workflow, V1WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (workflowInstance == null)
                throw new ArgumentNullException(nameof(workflowInstance));
            var fileName = this.Options.GetWorkerFileName();
            var args = null as string;
            if (this.Environment.IsDevelopment())
                args += "--debug";
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = this.Options.WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            startInfo.Environment.Add(EnvironmentVariables.Api.HostName.Name, EnvironmentVariables.Api.HostName.Value!); //todo: instead, fetch values from options
            startInfo.Environment.Add(EnvironmentVariables.Runtime.WorkflowInstanceId.Name, workflowInstance.Id.ToString());
            if (this.ApplicationOptions.SkipCertificateValidation)
                startInfo.Environment.Add(EnvironmentVariables.SkipCertificateValidation.Name, "true");
            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            return Task.FromResult<IWorkflowProcess>(new NativeProcess(process));
        }

        /// <summary>
        /// Handles the exit of a <see cref="Process"/>
        /// </summary>
        /// <param name="workflowInstanceId">The id of the <see cref="V1WorkflowInstance"/> the <see cref="Process"/> belongs to</param>
        /// <param name="process">The <see cref="Process"/> that has exited</param>
        protected virtual void OnProcessExited(string workflowInstanceId, Process process)
        {
            if (string.IsNullOrWhiteSpace(workflowInstanceId))
                throw new ArgumentNullException(nameof(workflowInstanceId));
            if (process == null)
                throw new ArgumentNullException(nameof(process));
            this.Processes.TryRemove(workflowInstanceId, out _);
            process.Dispose();
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsync()
        {
            foreach(var process in this.Processes)
            {
                process.Value.Dispose();
            }
            this.Processes.Clear();
            await base.DisposeAsync();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (var process in this.Processes)
            {
                process.Value.Dispose();
            }
            this.Processes.Clear();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

    }

}