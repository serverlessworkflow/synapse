namespace Synapse.Runtime.Services;

/// <summary>
/// Represents the native implementation of the <see cref="IWorkflowProcess"/>
/// </summary>
public class NativeProcess
    : WorkflowProcessBase
{

    /// <summary>
    /// Initializes a new <see cref="NativeProcess"/>
    /// </summary>
    /// <param name="process">The underlying <see cref="System.Diagnostics.Process"/></param>
    public NativeProcess(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);
        this.Id = Guid.NewGuid().ToString();
        this.Process = process;
        this.Process.Exited += (sender, e) => this.OnExited();
        this.StandardOutput = this.Process.GetStandardOutputAsObservable();
        this.StandardError = this.Process.GetStandardErrorAsObservable();
    }

    /// <inheritdoc/>
    public override string Id { get; }

    /// <summary>
    /// Gets the underlying <see cref="System.Diagnostics.Process"/>
    /// </summary>
    protected Process Process { get; }

    /// <inheritdoc/>
    public override IObservable<string> StandardOutput { get; }

    /// <inheritdoc/>
    public override IObservable<string> StandardError { get; }

    /// <inheritdoc/>
    public override long? ExitCode => this.Process.HasExited ? this.Process.ExitCode : null;

    /// <inheritdoc/>
    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.Process.Start();
        //this.Process.BeginOutputReadLine(); //todo: check why that was there and why it now throws
        //this.Process.BeginErrorReadLine();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        this.Process.Close();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        this.Process.Dispose();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.Process.Dispose();
        base.Dispose(disposing);
    }

}