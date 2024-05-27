namespace Synapse.Runtime;

/// <summary>
/// Defines extensions for <see cref="Process"/>es
/// </summary>
public static class ProcessExtensions
{

    /// <summary>
    /// Gets a new <see cref="IObservable{T}"/> to observe the <see cref="Process"/>'s Standard Output
    /// </summary>
    /// <param name="process">The <see cref="Process"/> to observe the logs of</param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the process's logs</returns>
    public static IObservable<string> GetStandardOutputAsObservable(this Process process) => Observable
        .FromEventPattern<DataReceivedEventHandler, DataReceivedEventArgs>(handler => process.OutputDataReceived += handler, handler => process.OutputDataReceived -= handler)
        .Where(l => !string.IsNullOrWhiteSpace(l?.EventArgs?.Data))
        .Select(l => l.EventArgs.Data!);

    /// <summary>
    /// Gets a new <see cref="IObservable{T}"/> to observe the <see cref="Process"/>'s Standard Error
    /// </summary>
    /// <param name="process">The <see cref="Process"/> to observe the logs of</param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe the process's logs</returns>
    public static IObservable<string> GetStandardErrorAsObservable(this Process process) => Observable
        .FromEventPattern<DataReceivedEventHandler, DataReceivedEventArgs>(handler => process.ErrorDataReceived += handler, handler => process.ErrorDataReceived -= handler)
        .Where(l => !string.IsNullOrWhiteSpace(l?.EventArgs?.Data))
        .Select(l => l.EventArgs.Data!);

}
