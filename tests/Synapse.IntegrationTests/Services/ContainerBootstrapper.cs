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

using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Hosting;

namespace Synapse.IntegrationTests.Services;

public class ContainerBootstrapper
    : IHostedService
{

    public ContainerBootstrapper(IContainer container) => this.Container = container;

    protected IContainer Container { get; }

    public Task StartAsync(CancellationToken cancellationToken) => Container.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
