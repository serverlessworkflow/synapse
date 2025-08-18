// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.Runtime.Services;

/// <summary>
/// Represents the native implementation of the <see cref="IWorkflowRuntime"/>
/// </summary>
/// <remarks>
/// Initializes a new <see cref="NativeRuntime"/>
/// </remarks>
/// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
/// <param name="environment">The current <see cref="IHostEnvironment"/></param>
/// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
/// <param name="options">The service used to access the current <see cref="RunnerConfiguration"/></param>
public class NativeRuntime(ILoggerFactory loggerFactory, IHostEnvironment environment, IHttpClientFactory httpClientFactory, IOptionsMonitor<RunnerConfiguration> options)
    : WorkflowRuntimeBase(loggerFactory)
{

    /// <summary>
    /// Gets the current <see cref="IHostEnvironment"/>
    /// </summary>
    protected IHostEnvironment Environment { get; } = environment;

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpClient"/> used to perform HTTP requests
    /// </summary>
    protected HttpClient HttpClient { get; } = httpClientFactory.CreateClient();

    /// <summary>
    /// Gets the current <see cref="RunnerConfiguration"/>
    /// </summary>
    protected RunnerConfiguration Options => options.CurrentValue;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all known worker processes
    /// </summary>
    protected ConcurrentDictionary<string, NativeProcess> Processes { get; } = new();

    /// <inheritdoc/>
    public override Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, ServiceAccount serviceAccount, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(workflowInstance);
        ArgumentNullException.ThrowIfNull(serviceAccount);
        if (this.Options.Runtime.Native == null) throw new NullReferenceException("The native runtime must be configured");
        var fileName = this.Options.Runtime.Native.Executable;
        var args = string.Empty;
        if (this.Environment.IsDevelopment()) args += "--debug";
        var filePath = Path.Combine(this.Options.Runtime.Native.Directory, fileName);
        var startInfo = new ProcessStartInfo()
        {
            FileName = filePath,
            Arguments = args,
            WorkingDirectory = this.Options.Runtime.Native.Directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Runner.Namespace] = workflowInstance.GetNamespace()!;
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Runner.Name] = $"{workflowInstance.GetName()}-{Guid.NewGuid().ToString("N")[..12].ToLowerInvariant()}";
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Api.Uri] = this.Options.Api.Uri.OriginalString;
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Runner.ContainerPlatform] = this.Options.ContainerPlatform;
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Runner.LifecycleEvents] = (this.Options.PublishLifecycleEvents ?? true).ToString();
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Secrets.Directory] = this.Options.Runtime.Native.SecretsDirectory;
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.ServiceAccount.Name] = serviceAccount.GetQualifiedName();
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.ServiceAccount.Key] = serviceAccount.Spec.Key;
        startInfo.Environment[SynapseDefaults.EnvironmentVariables.Workflow.Instance] = workflowInstance.GetQualifiedName();
        if (this.Options.Certificates?.Validate == false) startInfo.Environment.Add(SynapseDefaults.EnvironmentVariables.SkipCertificateValidation, "true");
        var process = new Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
        return Task.FromResult<IWorkflowProcess>(this.Processes.AddOrUpdate(workflowInstance.GetQualifiedName(), new NativeProcess(process), (key, current) => current));
    }

    /// <inheritdoc/>
    public override async Task DeleteProcessAsync(string processId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        if (this.Processes.TryRemove(processId, out var process)) await process.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the exit of a <see cref="Process"/>
    /// </summary>
    /// <param name="workflowInstanceQualifiedName">The id of the <see cref="WorkflowInstance"/> the <see cref="Process"/> belongs to</param>
    /// <param name="process">The <see cref="Process"/> that has exited</param>
    protected virtual void OnProcessExited(string workflowInstanceQualifiedName, Process process)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowInstanceQualifiedName);
        ArgumentNullException.ThrowIfNull(process);
        this.Processes.TryRemove(workflowInstanceQualifiedName, out _);
        process.Dispose();
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing) return;
        foreach (var process in this.Processes) process.Value.Dispose();
        this.Processes.Clear();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        foreach (var process in this.Processes)  process.Value.Dispose();
        this.Processes.Clear();
        base.Dispose(disposing);
    }

}
