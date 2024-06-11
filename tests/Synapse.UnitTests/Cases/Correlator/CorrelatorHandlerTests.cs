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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neuroglia;
using Neuroglia.Data.Expressions.JQ;
using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Eventing.CloudEvents.Infrastructure;
using Neuroglia.Security.Services;
using StackExchange.Redis;
using Synapse.Core.Infrastructure.Services;
using Synapse.Correlator.Configuration;
using Synapse.Correlator.Services;
using Synapse.UnitTests.Containers;
using System.Reactive.Linq;
using System.Text.Json.Nodes;

namespace Synapse.UnitTests.Cases.Correlator;

public class CorrelatorHandlerTests
    : IAsyncLifetime
{

    public CorrelatorHandlerTests()
    {
        RootServiceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();
        ServiceScope = RootServiceProvider.CreateScope();
    }

    protected ServiceProvider RootServiceProvider { get; }

    protected IServiceScope ServiceScope { get; }

    protected IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

    protected IResourceRepository Resources => ServiceProvider.GetRequiredService<IResourceRepository>();

    protected ICloudEventBus CloudEventBus => ServiceProvider.GetRequiredService<ICloudEventBus>();

    [Fact]
    public async Task Correlate_One_Event_Should_Work()
    {
        //arrange
        var correlationId = Guid.NewGuid().ToShortString();
        var e = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.temperature-measured.v1alpha1",
            Data = new
            {
                Temperature = 38
            }
        };
        e.ExtensionAttributes ??= new Dictionary<string, object>();
        e.ExtensionAttributes["correlationid"] = correlationId;
        var correlationKeyName = "thermometerId";
        var correlationKeyFrom = "correlationid";
        var eventFilter = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = correlationKeyFrom
                })
            ]
        };
        var correlation = await this.Resources.AddAsync<Correlation>(new()
        {
            Metadata = new()
            {
                Namespace = "default",
                Name = "test"
            },
            Spec = new()
            {
                Events = new()
                {
                    One = eventFilter
                }
            }
        });
        var correlationMonitor = await this.Resources.MonitorAsync<Correlation>(correlation.GetName(), correlation.GetNamespace());
        var correlator = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, [correlationMonitor]);

        //act
        await correlator.HandleAsync();
        this.CloudEventBus.InputStream.OnNext(e);
        await Task.Delay(150);

        //assert
        var corel = (await this.Resources.GetAsync<Correlation>(correlation.GetName(), correlation.GetNamespace()))!;
        corel.Status.Should().NotBeNull();
        corel.Status!.Contexts.Should().HaveCount(1);
        var context = corel.Status.Contexts.First();
        context.Keys.Should().ContainKey(correlationKeyName);
        context.Keys[correlationKeyName].Should().Be(correlationId);
        context.Events.Should().ContainSingle();
    }

    [Fact]
    public async Task Correlate_One_Event_With_Matching_Key_From_Attribute_With_Constant_Expectation_Should_Work()
    {
        //arrange
        var correlationId = Guid.NewGuid().ToShortString();
        var e = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.temperature-measured.v1alpha1",
            Data = new
            {
                Temperature = 38
            }
        };
        e.ExtensionAttributes ??= new Dictionary<string, object>();
        e.ExtensionAttributes["correlationid"] = correlationId;
        var correlationKeyName = "thermometerId";
        var correlationKeyFrom = "correlationid";
        var eventFilter = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = correlationKeyFrom,
                    Expect = correlationId
                })
            ]
        };
        var correlation = await this.Resources.AddAsync<Correlation>(new()
        {
            Metadata = new()
            {
                Namespace = "default",
                Name = "test"
            },
            Spec = new()
            {
                Events = new()
                {
                    One = eventFilter
                }
            }
        });
        var correlationMonitor = await this.Resources.MonitorAsync<Correlation>(correlation.GetName(), correlation.GetNamespace());
        var correlator = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, [correlationMonitor]);

        //act
        await correlator.HandleAsync();
        this.CloudEventBus.InputStream.OnNext(e);
        await Task.Delay(150);

        //assert
        var corel = (await this.Resources.GetAsync<Correlation>(correlation.GetName(), correlation.GetNamespace()))!;
        corel.Status.Should().NotBeNull();
        corel.Status!.Contexts.Should().HaveCount(1);
        var context = corel.Status.Contexts.First();
        context.Keys.Should().ContainKey(correlationKeyName);
        context.Keys[correlationKeyName].Should().Be(correlationId);
        context.Events.Should().ContainSingle();
    }

    [Fact]
    public async Task Correlate_One_Event_With_Different_Key_From_Attribute_With_Constant_Expectation_Should_Fail()
    {
        //arrange
        var correlationId = Guid.NewGuid().ToShortString();
        var e = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.temperature-measured.v1alpha1",
            Data = new
            {
                Temperature = 38
            }
        };
        e.ExtensionAttributes ??= new Dictionary<string, object>();
        e.ExtensionAttributes["correlationid"] = correlationId;
        var correlationKeyName = "thermometerId";
        var correlationKeyFrom = "correlationid";
        var eventFilter = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = correlationKeyFrom,
                    Expect = Guid.NewGuid().ToShortString()
                })
            ]
        };
        var correlation = await this.Resources.AddAsync<Correlation>(new()
        {
            Metadata = new()
            {
                Namespace = "default",
                Name = "test"
            },
            Spec = new()
            {
                Events = new()
                {
                    One = eventFilter
                }
            }
        });
        var correlationMonitor = await this.Resources.MonitorAsync<Correlation>(correlation.GetName(), correlation.GetNamespace());
        var correlator = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, [correlationMonitor]);

        //act
        await correlator.HandleAsync();
        this.CloudEventBus.InputStream.OnNext(e);
        await Task.Delay(150);

        //assert
        var corel = (await this.Resources.GetAsync<Correlation>(correlation.GetName(), correlation.GetNamespace()))!;
        corel.Status.Should().NotBeNull();
        corel.Status!.Contexts.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Correlate_One_Event_With_Matching_Key_From_Attribute__With_Runtime_Expectation_Should_Work()
    {
        //arrange
        var correlationId = Guid.NewGuid().ToShortString();
        var e = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.temperature-measured.v1alpha1",
            Data = new
            {
                Temperature = 38
            }
        };
        e.ExtensionAttributes ??= new Dictionary<string, object>();
        e.ExtensionAttributes["correlationid"] = correlationId;
        var correlationKeyName = "thermometerId";
        var correlationKeyFrom = "correlationid";
        var eventFilter = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = correlationKeyFrom,
                    Expect = @$"${{ . == ""{correlationId}"" }}" 
                })
            ]
        };
        var correlation = await this.Resources.AddAsync<Correlation>(new()
        {
            Metadata = new()
            {
                Namespace = "default",
                Name = "test"
            },
            Spec = new()
            {
                Events = new()
                {
                    One = eventFilter
                }
            }
        });
        var correlationMonitor = await this.Resources.MonitorAsync<Correlation>(correlation.GetName(), correlation.GetNamespace());
        var correlator = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, [correlationMonitor]);

        //act
        await correlator.HandleAsync();
        this.CloudEventBus.InputStream.OnNext(e);
        await Task.Delay(150);

        //assert
        var corel = (await this.Resources.GetAsync<Correlation>(correlation.GetName(), correlation.GetNamespace()))!;
        corel.Status.Should().NotBeNull();
        corel.Status!.Contexts.Should().HaveCount(1);
        var context = corel.Status.Contexts.First();
        context.Keys.Should().ContainKey(correlationKeyName);
        context.Keys[correlationKeyName].Should().Be(correlationId);
        context.Events.Should().ContainSingle();
    }

    [Fact]
    public async Task Correlate_Any_Event_Should_Work()
    {

    }

    [Fact]
    public async Task Correlate_Any_Event_Until_Should_Work()
    {

    }

    [Fact]
    public async Task Correlate_All_Events_Should_Work()
    {
        //arrange
        var correlationId = Guid.NewGuid().ToShortString();
        var e1 = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.temperature-measured.v1alpha1",
            Data = new
            {
                Temperature = 38
            }
        };
        e1.ExtensionAttributes ??= new Dictionary<string, object>();
        e1.ExtensionAttributes["correlationid"] = correlationId;
        var e2 = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.light-measured.v1alpha1",
            Data = new
            {
                Lumens = 1589
            }
        };
        e2.ExtensionAttributes ??= new Dictionary<string, object>();
        e2.ExtensionAttributes["roomid"] = correlationId;
        var e3 = new CloudEvent()
        {
            SpecVersion = CloudEventSpecVersion.V1.Version,
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new("https://unit-tests.net-sdk.serverlessworkflow.io"),
            Type = "io.serverlessworkflow.unit-tests.events.humidity-measured.v1alpha1",
            Data = new
            {
                RoomId = correlationId,
                HumidityLevel = 0.66
            }
        };
        e3.ExtensionAttributes ??= new Dictionary<string, object>();
        var correlationKeyName = "roomId";
        var eventFilter1 = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e1.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = "correlationid"
                })
            ]
        };
        var eventFilter2 = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e2.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = "roomid"
                })
            ]
        };
        var eventFilter3 = new EventFilterDefinition()
        {
            With =
            [
                new(CloudEventAttributes.Type, e3.Type)
            ],
            Correlate =
            [
                new(correlationKeyName, new()
                {
                    From = "${ .data.roomId }"
                })
            ]
        };
        await this.Resources.AddNamespaceAsync("fake-ns");
        var correlation = await this.Resources.AddAsync<Correlation>(new()
        {
            Metadata = new()
            {
                Namespace = "fake-ns",
                Name = "test"
            },
            Spec = new()
            {
                Events = new()
                {
                    All = [eventFilter1, eventFilter2, eventFilter3]
                }
            }
        });
        await using var correlationMonitor = await this.Resources.MonitorAsync<Correlation>(correlation.GetName(), correlation.GetNamespace(), true);
        var correlator = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, [correlationMonitor]);

        //act
        await correlator.HandleAsync();
        this.CloudEventBus.InputStream.OnNext(e1);
        await Task.Delay(150);
        this.CloudEventBus.InputStream.OnNext(e2);
        await Task.Delay(150);
        this.CloudEventBus.InputStream.OnNext(e3);
        await Task.Delay(150);

        //assert
        var corel = (await this.Resources.GetAsync<Correlation>(correlation.GetName(), correlation.GetNamespace()))!;
        corel.Status.Should().NotBeNull();
        corel.Status!.Contexts.Should().HaveCount(1);
        var context = corel.Status.Contexts.First();
        context.Keys.Should().ContainKey(correlationKeyName);
        context.Keys[correlationKeyName].Should().Be(correlationId);
        context.Events.Count.Should().Be(3);
    }

    public async Task InitializeAsync()
    {
        foreach (var service in this.ServiceProvider.GetServices<IHostedService>()) await service.StartAsync(default);
    }

    public async Task DisposeAsync()
    {
        this.ServiceScope.Dispose();
        await RootServiceProvider.DisposeAsync();
    }

    protected virtual IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder().Build();
        services.AddHostedService<ContainerBootstrapper>();
        services.Configure<CorrelatorOptions>(options =>
        {
            options.Namespace = "default";
            options.Name = "test";
        });
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddSingleton<IUserAccessor, ApplicationUserAccessor>();
        services.AddJQExpressionEvaluator();
        services.AddSynapse(configuration);
        services.AddKeyedSingleton("redis", RedisContainerBuilder.Build());
        services.AddSingleton(provider => provider.GetRequiredKeyedService<DotNet.Testcontainers.Containers.IContainer>("redis"));
        services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect($"localhost:{provider.GetRequiredKeyedService<DotNet.Testcontainers.Containers.IContainer>("redis").GetMappedPublicPort(RedisContainerBuilder.PublicPort)}"));
        services.AddScoped<Neuroglia.Data.Infrastructure.ResourceOriented.Services.IDatabase, RedisDatabase>();
        services.AddHostedService<Core.Infrastructure.Services.DatabaseInitializer>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddRedisRepository<Document, string>(lifetime: ServiceLifetime.Scoped);
        services.AddCloudEventBus();
        return services;
    }

}
