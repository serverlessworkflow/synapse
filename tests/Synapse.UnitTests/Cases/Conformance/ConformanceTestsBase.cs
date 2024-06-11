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
using Gherkin.Ast;
using Json.Pointer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Neuroglia;
using Neuroglia.Data.Expressions.JQ;
using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Neuroglia.Data.PatchModel.Services;
using Neuroglia.Eventing.CloudEvents.Infrastructure;
using Neuroglia.Security.Services;
using ServerlessWorkflow.Sdk.IO;
using StackExchange.Redis;
using Synapse.Api.Client.Services;
using Synapse.Core.Infrastructure.Containers;
using Synapse.Core.Infrastructure.Services;
using Synapse.Runner.Services;
using Synapse.UnitTests.Containers;
using System.Text;
using Xunit.Gherkin.Quick;

namespace Synapse.UnitTests.Cases.Conformance;

public abstract class ConformanceTestsBase
    : Xunit.Gherkin.Quick.Feature, IAsyncLifetime
{

    public ConformanceTestsBase() => Services = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

    protected ServiceProvider Services { get; }

    protected IResourceRepository Resources => Services.GetRequiredService<IResourceRepository>();

    protected IRepository<Document, string> Documents => Services.GetRequiredService<IRepository<Document, string>>();

    protected IJsonSerializer JsonSerializer => Services.GetRequiredService<IJsonSerializer>();

    protected IYamlSerializer YamlSerializer => Services.GetRequiredService<IYamlSerializer>();

    protected WorkflowDefinition Definition { get; set; } = null!;

    protected Workflow Resource { get; set; } = null!;

    protected EquatableDictionary<string, object>? Input { get; set; }

    protected WorkflowInstance Instance { get; set; } = null!;

    protected IWorkflowExecutionContext ExecutionContext { get; set; } = null!;

    public async Task InitializeAsync()
    {
        foreach (var service in Services.GetServices<IHostedService>()) await service.StartAsync(default);
    }

    public async Task DisposeAsync() => await Services.DisposeAsync();

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
        services.AddServerlessWorkflowIO();
        return services;
    }

    [Given(@"a workflow with definition:")]
    public async Task Given_A_Workflow_Definition(DocString inputString)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(inputString.Content));
        Definition = await Services.GetRequiredService<IWorkflowDefinitionReader>().ReadAsync(stream);
        Resource = await Resources.AddAsync(new Workflow()
        {
            Metadata = new()
            {
                Namespace = Definition.Document.Namespace,
                Name = Definition.Document.Name
            },
            Spec = new()
            {
                Versions = [ Definition ]
            }
        });
    }

    [And(@"given the workflow input is:")]
    public void Given_The_Workflow_Input_Is(DocString inputString)
    {
        Input = YamlSerializer.Deserialize<EquatableDictionary<string, object>>(inputString.Content);
    }

    [When(@"the workflow is executed")]
    public async Task When_The_Workflow_Is_Executed()
    {
        Instance = await Resources.AddAsync(new WorkflowInstance()
        {
            Metadata = new()
            {
                Namespace = Definition.Document.Namespace,
                Name = $"{Definition.Document.Name}-{Guid.NewGuid().ToString("N")[..15]}"
            },
            Spec = new()
            {
                Definition = $"{Definition.Document.Namespace}.{Definition.Document.Name}:{Definition.Document.Version}",
                Input = Input
            }
        });
        ExecutionContext = ActivatorUtilities.CreateInstance<WorkflowExecutionContext>(Services, Definition, Instance);
        var executor = ActivatorUtilities.CreateInstance<WorkflowExecutor>(Services, ExecutionContext);
        await executor.ExecuteAsync();
    }

    [Then(@"the workflow should complete")]
    public void Then_Workflow_Should_Complete()
    {
        ExecutionContext.Instance.Status?.Phase.Should().Be(WorkflowInstanceStatusPhase.Completed);
    }

    [Then(@"the workflow should complete with output:")]
    public void Then_Workflow_Should_Complete_With_Output(DocString outputString)
    {
        var expectedOutput = ExecutionContext.Output switch
        {
            IDictionary<string, object> => JsonSerializer.Convert(YamlSerializer.Deserialize<object>(outputString.Content), typeof(EquatableDictionary<string, object>)),
            IEnumerable<object> => YamlSerializer.Deserialize<EquatableList<object>>(outputString.Content),
            null => null,
            _ => YamlSerializer.Deserialize(outputString.Content, ExecutionContext.Output?.GetType() ?? typeof(object))
        };
        var actualOutput = ExecutionContext.Output switch
        {
            IDictionary<string, object> => ExecutionContext.Output.ConvertTo<EquatableDictionary<string, object>>(),
            IEnumerable<object> => ExecutionContext.Output.ConvertTo<EquatableList<object>>()!,
            null => null,
            _ => ExecutionContext.Output
        };
        actualOutput.Should().BeEquivalentTo(expectedOutput);
    }

    [Then(@"the workflow should fault")]
    public void Then_Workflow_Should_Fault()
    {
        ExecutionContext.Instance.Status?.Phase.Should().Be(WorkflowInstanceStatusPhase.Faulted);
        ExecutionContext.Instance.Status?.Error.Should().NotBeNull();
    }

    [Then(@"the workflow should fault with error:")]
    public void Then_Workflow_Should_Fault_With_Error(DocString outputString)
    {
        ExecutionContext.Instance.Status?.Phase.Should().Be(WorkflowInstanceStatusPhase.Faulted);
        ExecutionContext.Instance.Status?.Error.Should().NotBeNull();
        var error = YamlSerializer.Deserialize<Error>(outputString.Content);
        ExecutionContext.Instance.Status?.Error.Should().BeEquivalentTo(error);
    }

    [And(@"the workflow output should have properties (.*)")]
    public void Then_Workflow_Output_Should_Have_Properties(string propertiesString)
    {
        this.Then_Workflow_Should_Complete();
        var output = JsonSerializer.SerializeToNode(ExecutionContext.Output);
        var properties = propertiesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => JsonPointer.Parse($"/{p.Trim()[1..^1].Replace('.', '/')}"));
        output.Should().NotBeNull();
        properties.Should().AllSatisfy(p => p.TryEvaluate(output, out _).Should().BeTrue());
    }

    [And("the workflow output should have a '(.*)' property with value:")]
    public void Then_Workflow_Output_Should_Have_Property_With_Value(string propertyPath, DocString propertyValueString)
    {
        this.Then_Workflow_Should_Complete();
        var propertyValueJson = JsonSerializer.SerializeToText(YamlSerializer.Deserialize<object>(propertyValueString.Content));
        var output = JsonSerializer.SerializeToNode(ExecutionContext.Output)?.AsObject();
        output.Should().NotBeNull();
        var pointer = JsonPointer.Parse($"/{propertyPath.Trim().Replace('.', '/')}");
        pointer.TryEvaluate(output, out var outputPropertyValue).Should().BeTrue();
        outputPropertyValue?.ToJsonString().Should().Be(propertyValueJson);
    }

    [And("the workflow output should have a '(.*)' property containing (.*) items")]
    public void Then_Workflow_Output_Should_Have_Property_With_Count(string propertyPath, int count)
    {
        this.Then_Workflow_Should_Complete();
        var output = JsonSerializer.SerializeToNode(ExecutionContext.Output);
        output.Should().NotBeNull();
        var pointer = JsonPointer.Parse($"/{propertyPath.Trim().Replace('.', '/')}");
        pointer.TryEvaluate(output, out var outputPropertyValue).Should().BeTrue();
        outputPropertyValue!.AsArray()!.Should().HaveCount(count);
    }

    [And(@"(.*) should run first")]
    public void Then_Task_Should_Run_First(string taskName)
    {
        var first = ExecutionContext.Instance.Status?.Tasks?.FirstOrDefault();
        first.Should().NotBeNull();
        first!.Name.Should().Be(taskName);
    }

    [And(@"(.*) should run after (.*)")]
    public void Then_Task_Should_Run_After(string task1Name, string task2Name)
    {
        var task1 = ExecutionContext.Instance.Status?.Tasks?.FirstOrDefault(t => t.Name == task1Name);
        task1.Should().NotBeNull();
        var task2 = ExecutionContext.Instance.Status?.Tasks?.FirstOrDefault(t => t.Name == task2Name);
        task2.Should().NotBeNull();
        var index1 = ExecutionContext.Instance.Status!.Tasks!.IndexOf(task1!);
        var index2 = ExecutionContext.Instance.Status!.Tasks!.IndexOf(task2!);
        index1.Should().BeGreaterThan(index2);
    }

    [And(@"(.*) should complete")]
    public async Task Then_Task_Should_Complete(string taskName, DocString outputString)
    {
        var task = ExecutionContext.Instance.Status?.Tasks?.FirstOrDefault(t => t.Name == taskName);
        task.Should().NotBeNull();
        task!.OutputReference.Should().NotBeNullOrWhiteSpace();
        var outputDocument = await Documents.GetAsync(task.OutputReference!);
        outputDocument.Should().NotBeNull();
        var output = YamlSerializer.Deserialize<EquatableDictionary<string, object>>(outputString.Content);
        outputDocument!.Content.Should().BeEquivalentTo(output);
    }

    [And(@"(.*) should complete with output:")]
    public async Task Then_Task_Should_Complete_With_Output(string taskName, DocString outputString)
    {
        var task = ExecutionContext.Instance.Status?.Tasks?.FirstOrDefault(t => t.Name == taskName);
        task.Should().NotBeNull();
        task!.OutputReference.Should().NotBeNullOrWhiteSpace();
        var outputDocument = await Documents.GetAsync(task.OutputReference!);
        outputDocument.Should().NotBeNull();
        var output = YamlSerializer.Deserialize<EquatableDictionary<string, object>>(outputString.Content);
        outputDocument!.Content.Should().BeEquivalentTo(output);
    }

    [And(@"(.*) should run last")]
    public void Then_Task_Should_Run_Last(string taskName)
    {
        var last = ExecutionContext.Instance.Status?.Tasks?.LastOrDefault();
        last.Should().NotBeNull();
        last!.Name.Should().Be(taskName);
    }

}
