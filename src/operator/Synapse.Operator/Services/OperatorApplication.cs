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

namespace Synapse.Operator.Services;

internal class OperatorApplication(IServiceProvider serviceProvider)
    : IHostedService, IDisposable
{

    readonly IServiceScope _scope = serviceProvider.CreateScope();
    IServiceProvider ServiceProvider => this._scope.ServiceProvider;

    OperatorController _operatorController = null!;
    WorkflowController _workflowController = null!;
    WorkflowInstanceController _workflowInstanceController = null!;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._operatorController = this.ServiceProvider.GetRequiredService<OperatorController>();
        this._workflowController = this.ServiceProvider.GetRequiredService<WorkflowController>();
        this._workflowInstanceController = this.ServiceProvider.GetRequiredService<WorkflowInstanceController>();
        await this._operatorController.StartAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(
        [
            this._workflowController.StartAsync(cancellationToken),
            this._workflowInstanceController.StartAsync(cancellationToken)
        ]).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken) 
    {
        await Task.WhenAll(
        [
            this._operatorController.StopAsync(cancellationToken),
            this._workflowController.StopAsync(cancellationToken),
            this._workflowInstanceController.StopAsync(cancellationToken)
        ]).ConfigureAwait(false);
    }

    void IDisposable.Dispose() => this._scope.Dispose();

}