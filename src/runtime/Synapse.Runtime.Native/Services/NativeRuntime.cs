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
/// <param name="options">The service used to access the current <see cref="NativeRuntimeOptions"/></param>
public class NativeRuntime(ILoggerFactory loggerFactory, IHostEnvironment environment, IHttpClientFactory httpClientFactory, IOptions<NativeRuntimeOptions> options)
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
    /// Gets the current <see cref="NativeRuntimeOptions"/>
    /// </summary>
    protected NativeRuntimeOptions Options { get; } = options.Value;

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all known worker processes
    /// </summary>
    protected ConcurrentDictionary<string, Process> Processes { get; } = new();

    /// <summary>
    /// Downloads and installs the worker binaries, if not already present
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task InstallWorkerAsync(CancellationToken cancellationToken)
    {
        this.Logger.LogInformation("Downloading the Runner application...");
        var workerDirectory = new DirectoryInfo(this.Options.WorkingDirectory);
        if (!workerDirectory.Exists) workerDirectory.Create();
        if (File.Exists(this.Options.GetWorkerFileName()))
        {
            this.Logger.LogInformation("Runner application already present locally. Skipping download."); //todo: config based: the user might want to get latest every time
            return;
        }
        string? target;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) target = "win-x64.zip";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) target = "linux-x64.tar.gz";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) target = "osx-x64.tar.gz";
        else throw new PlatformNotSupportedException();
        using var packageStream = await this.HttpClient.GetStreamAsync($"https://github.com/serverlessworkflow/synapse/releases/download/v{typeof(NativeRuntime).Assembly.GetName().Version!.ToString(3)!}/synapse-worker-{target}", cancellationToken); //todo: config based
        using ZipArchive archive = new(packageStream, ZipArchiveMode.Read);
        this.Logger.LogInformation("Runner application successfully downloaded. Extracting...");
        archive.ExtractToDirectory(workerDirectory.FullName, true);
        this.Logger.LogInformation("Runner application successfully extracted");
    }

    /// <inheritdoc/>
    public override Task<IWorkflowProcess> CreateProcessAsync(Workflow workflow, WorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(workflowInstance);
        var fileName = this.Options.GetWorkerFileName();
        var args = string.Empty;
        if (this.Environment.IsDevelopment()) args += "--debug";
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
        startInfo.Environment.Add(SynapseDefaults.EnvironmentVariables.Api.Uri, this.Options.Api.Uri.OriginalString);
        startInfo.Environment.Add(SynapseDefaults.EnvironmentVariables.Workflow.Instance, workflowInstance.GetQualifiedName());
        if (this.Options.SkipCertificateValidation) startInfo.Environment.Add(SynapseDefaults.EnvironmentVariables.SkipCertificateValidation, "true");
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
    /// <param name="workflowInstanceQualifiedName">The id of the <see cref="V1WorkflowInstance"/> the <see cref="Process"/> belongs to</param>
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
