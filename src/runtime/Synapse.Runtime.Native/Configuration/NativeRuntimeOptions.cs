namespace Synapse.Runtime.Configuration;

/// <summary>
/// Represents the options used to configure a Synapse Docker-based runtime
/// </summary>
public class NativeRuntimeOptions
    : RuntimeOptions
{

    /// <summary>
    /// Gets the name of the default runner file
    /// </summary>
    public const string DefaultRunnerExecutable = "Synapse.Runner";

    /// <summary>
    /// Gets/sets the name of the worker executable file to run
    /// </summary>
    public virtual string RunnerExecutable { get; set; } = DefaultRunnerExecutable;

    /// <summary>
    /// Gets/sets the directory in which to run the worker process
    /// </summary>
    public virtual string WorkingDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "bin", "runner");

    /// <summary>
    /// Gets the full name of the worker file to run
    /// </summary>
    /// <returns>The full name of the worker file to run</returns>
    public virtual string GetWorkerFileName()
    {
        var directory = this.WorkingDirectory;
        var fileName = this.RunnerExecutable;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) fileName += ".exe";
        return Path.Combine(directory, fileName);
    }

}
