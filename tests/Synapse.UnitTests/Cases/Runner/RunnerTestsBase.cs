using Docker.DotNet;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Moq;
using Neuroglia.Data.Expressions.JQ;
using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Neuroglia.Data.PatchModel.Services;
using Neuroglia.Eventing.CloudEvents.Infrastructure;
using Neuroglia.Security.Services;
using StackExchange.Redis;
using Synapse.Api.Client.Services;
using Synapse.Core.Infrastructure.Containers;
using Synapse.Core.Infrastructure.Services;
using Synapse.Runner.Services;
using Synapse.UnitTests.Containers;

namespace Synapse.UnitTests.Cases.Runner;

public abstract class RunnerTestsBase
    : IAsyncLifetime
{

    public RunnerTestsBase() => ServiceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

    protected ServiceProvider ServiceProvider { get; }

    protected IResourceRepository Resources => ServiceProvider.GetRequiredService<IResourceRepository>();

    protected IRepository<Document> Documents => ServiceProvider.GetRequiredService<IRepository<Document>>();

    public async Task InitializeAsync()
    {
        foreach (var service in this.ServiceProvider.GetServices<IHostedService>()) await service.StartAsync(default);
    }

    public async Task DisposeAsync() => await ServiceProvider.DisposeAsync();

    protected virtual IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSerialization();
        services.AddJsonSerializer();
        services.AddJQExpressionEvaluator();
        services.AddSingleton<ITaskExecutionContextFactory, TaskExecutionContextFactory>();
        services.AddSingleton<ITaskExecutorFactory, TaskExecutorFactory>();
        services.AddMemoryCacheRepository<Document, string>();
        services.AddScoped<IResourceRepository, MockResourceRepository>();
        services.AddCloudEventBus();
        services.AddHttpClient();
        services.AddSingleton<IContainerPlatform, DockerContainerPlatform>();
        services.AddSingleton<IDockerClient>(new DockerClientConfiguration().CreateClient());
        services.AddSingleton<IExternalResourceProvider, ExternalResourceProvider>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IAdmissionControl, AdmissionControl>();
        services.AddScoped<IVersionControl, VersionControl>();
        services.AddSingleton<IPatchHandler, JsonMergePatchHandler>();
        services.AddSingleton<IPatchHandler, JsonPatchHandler>();
        services.AddSingleton<IPatchHandler, JsonStrategicMergePatchHandler>();
        services.AddKeyedSingleton("redis", RedisContainerBuilder.Build());
        services.AddSingleton(provider => provider.GetRequiredKeyedService<DotNet.Testcontainers.Containers.IContainer>("redis"));
        services.AddHostedService<ContainerBootstrapper>();
        services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect($"localhost:{provider.GetRequiredKeyedService<DotNet.Testcontainers.Containers.IContainer>("redis").GetMappedPublicPort(RedisContainerBuilder.PublicPort)}"));
        services.AddScoped<Neuroglia.Data.Infrastructure.ResourceOriented.Services.IDatabase, RedisDatabase>();
        services.AddHostedService<Core.Infrastructure.Services.DatabaseInitializer>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddKeyedSingleton("mongo", MongoContainerBuilder.Build());
        services.AddSingleton(provider => provider.GetRequiredKeyedService<DotNet.Testcontainers.Containers.IContainer>("mongo"));
        services.AddSingleton<IMongoClient>(provider => new MongoClient(MongoClientSettings.FromConnectionString($"mongodb://{MongoContainerBuilder.DefaultUserName}:{MongoContainerBuilder.DefaultPassword}@localhost:{provider.GetRequiredKeyedService<DotNet.Testcontainers.Containers.IContainer>("mongo").GetMappedPublicPort(MongoContainerBuilder.PublicPort)}")));
        services.AddMongoDatabase("test");
        services.AddMongoRepository<Document, string>(lifetime: ServiceLifetime.Scoped);
        services.AddSingleton<ISynapseApiClient, MockSynapseApiClient>();
        services.AddSingleton(new Mock<IUserInfoProvider>().Object);
        return services;
    }

}