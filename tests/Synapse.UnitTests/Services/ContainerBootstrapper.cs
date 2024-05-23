using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Hosting;

namespace Synapse.UnitTests.Services;

public class ContainerBootstrapper(IEnumerable<IContainer> containers)
    : IHostedService
{

    protected IEnumerable<IContainer> Containers { get; } = containers;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(this.Containers.Select(c => c.StartAsync(cancellationToken)));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
