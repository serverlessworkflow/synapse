// Copyright © 2024-Present Neuroglia SRL. All rights reserved.
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

namespace Synapse.Correlator.Services;

/// <summary>
/// Represents the correlator's application service
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
public class CorrelatorApplication(IServiceProvider serviceProvider)
    : IHostedService, IDisposable
{

    readonly IServiceScope _scope = serviceProvider.CreateScope();
    IServiceProvider ServiceProvider => this._scope.ServiceProvider;

    CorrelatorController _correlatorController = null!;
    CorrelationController _correlationController = null!;

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._correlatorController = this.ServiceProvider.GetRequiredService<CorrelatorController>();
        this._correlationController = this.ServiceProvider.GetRequiredService<CorrelationController>();
        await this._correlatorController.StartAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(
        [
            this._correlatorController.StartAsync(cancellationToken),
            this._correlationController.StartAsync(cancellationToken),
        ]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken) 
    {
        await Task.WhenAll(
        [
            this._correlatorController.StopAsync(cancellationToken),
        ]).ConfigureAwait(false);
    }

    void IDisposable.Dispose()
    {
        this._scope.Dispose();
        GC.SuppressFinalize(this);
    }

}