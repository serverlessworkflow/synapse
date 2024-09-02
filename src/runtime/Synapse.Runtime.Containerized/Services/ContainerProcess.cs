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

using Synapse.Core.Infrastructure.Services;
using System.Reactive.Linq;

namespace Synapse.Runtime.Services;

/// <summary>
/// Represents the container implementation of the <see cref="IWorkflowProcess"/>
/// </summary>

public class ContainerProcess
    : WorkflowProcessBase
{

    /// <summary>
    /// Initializes a new <see cref="ContainerProcess"/>
    /// </summary>
    /// <param name="container">The underlying <see cref="IContainer"/></param>
    public ContainerProcess(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        this.Id = Guid.NewGuid().ToString();
        this.Container = container;
    }

    /// <inheritdoc/>
    public override string Id { get; }

    /// <summary>
    /// Gets the underlying <see cref="IContainer"/>
    /// </summary>
    protected IContainer Container { get; }

    IObservable<string>? _standardOutput;
    /// <inheritdoc/>
    public override IObservable<string>? StandardOutput => this._standardOutput;

    IObservable<string>? _standardError;
    /// <inheritdoc/>
    public override IObservable<string>? StandardError => this._standardError;

    long? _exitCode;
    /// <inheritdoc/>
    public override long? ExitCode => this._exitCode;

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await this.Container.StartAsync(cancellationToken).ConfigureAwait(false);
        this._standardOutput = Observable.FromAsync(async () => (await this.Container.StandardOutput!.ReadLineAsync(cancellationToken).ConfigureAwait(false))!).Repeat().TakeWhile(line => line != null);
        this._standardError = Observable.FromAsync(async () => (await this.Container.StandardError!.ReadLineAsync(cancellationToken).ConfigureAwait(false))!).Repeat().TakeWhile(line => line != null);
        _ = this.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the container to complete
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await this.Container.WaitForExitAsync(cancellationToken);
            this._exitCode = this.Container.ExitCode;
            this.OnExited();
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.Container.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await this.Container.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.Container.Dispose();
        base.Dispose(disposing);
    }
}