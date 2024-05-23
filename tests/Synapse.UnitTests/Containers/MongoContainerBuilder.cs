using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Synapse.UnitTests.Containers;

public static class MongoContainerBuilder
{

    public const string DefaultUserName = "test";
    public const string DefaultPassword = "test";
    public const int PublicPort = 27017;

    public static IContainer Build()
    {
        return new ContainerBuilder()
            .WithName($"mongo-{Guid.NewGuid():N}")
            .WithImage("mongo:latest")
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", DefaultUserName)
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", DefaultPassword)
            .WithPortBinding(PublicPort, true)
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilPortIsAvailable(PublicPort))
            .Build();
    }

}