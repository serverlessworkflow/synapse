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

using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        services.AddSingleton<DockerContainerPlatform>();
        services.AddSingleton<IContainerPlatform>(provider => provider.GetRequiredService<DockerContainerPlatform>());
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DockerContainerPlatform>());
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
        services.AddRedisRepository<Document, string>(lifetime: ServiceLifetime.Scoped);
        services.AddSingleton<ISynapseApiClient, MockSynapseApiClient>();
        services.AddSingleton(new Mock<IUserInfoProvider>().Object);
        return services;
    }

}