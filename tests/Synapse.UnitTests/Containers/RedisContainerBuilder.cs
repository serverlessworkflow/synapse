using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Synapse.UnitTests.Containers;

public static class RedisContainerBuilder
{

    public const int PublicPort = 6379;

    public static IContainer Build()
    {
        return new ContainerBuilder()
            .WithName($"redis-{Guid.NewGuid():N}")
            .WithImage("redis")
            .WithPortBinding(PublicPort, true)
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilPortIsAvailable(PublicPort)
                .UntilMessageIsLogged("Ready to accept connections"))
            .Build();
    }

}
