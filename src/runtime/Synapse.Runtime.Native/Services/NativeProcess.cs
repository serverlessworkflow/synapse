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