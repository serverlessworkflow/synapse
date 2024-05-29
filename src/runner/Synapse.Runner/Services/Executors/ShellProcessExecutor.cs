using System.Net;

namespace Synapse.Runner.Services.Executors;

/// <summary>
/// Represents an <see cref="ITaskExecutor"/> implementation used to execute <see cref="ShellProcessDefinition"/>s
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="executionContextFactory">The service used to create <see cref="ITaskExecutionContext"/>s</param>
/// <param name="executorFactory">The service used to create <see cref="ITaskExecutor"/>s</param>
/// <param name="context">The current <see cref="ITaskExecutionContext"/></param>
/// <param name="serializer">The service used to serialize/deserialize objects to/from JSON</param>
public class ShellProcessExecutor(IServiceProvider serviceProvider, ILogger<ShellProcessExecutor> logger, ITaskExecutionContextFactory executionContextFactory, ITaskExecutorFactory executorFactory, ITaskExecutionContext<RunTaskDefinition> context, IJsonSerializer serializer)
    : TaskExecutor<RunTaskDefinition>(serviceProvider, logger, executionContextFactory, executorFactory, context, serializer)
{

    /// <summary>
    /// Gets the definition of the shell process to run
    /// </summary>
    protected ShellProcessDefinition ProcessDefinition => this.Task.Definition.Run.Shell!;

    /// <inheritdoc/>
    protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo("cmd.exe", ["/c", this.ProcessDefinition.Command, .. this.ProcessDefinition.Arguments ?? []])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(startInfo) ?? throw new NullReferenceException($"Failed to create the shell process defined at '{this.Task.Instance.Reference}'");
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var rawOutput = (await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
        var errorMessage = (await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
        if(process.ExitCode == 0) await this.SetResultAsync(rawOutput, this.Task.Definition.Then, cancellationToken).ConfigureAwait(false);
        else await this.SetErrorAsync(new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Type = ErrorType.Runtime,
            Title = ErrorTitle.Runtime,
            Detail = errorMessage,
            Instance = this.Task.Instance.Reference
        }, cancellationToken).ConfigureAwait(false);
    }

}
