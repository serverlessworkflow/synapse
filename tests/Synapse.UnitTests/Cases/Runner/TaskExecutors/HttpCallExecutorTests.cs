using Json.Pointer;
using Neuroglia;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class HttpCallExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Post_And_Read_Json_Should_Work()
    {
        //arrange
        var parameters = (EquatableDictionary<string, object>)JsonSerializer.Default.Convert(new HttpCallDefinition()
        {
            Method = "post",
            Endpoint = new() { Uri = new("https://petstore.swagger.io/v2/pet") },
            Body = new
            {
                name = "fake-name"
            }
        }, typeof(EquatableDictionary<string, object>))!;
        var taskDefinition = new CallTaskDefinition()
        {
            Call = Function.Http,
            With = parameters
        };
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Namespace = "default",
                Name = "fake",
                Version = "0.1.0"
            },
            Do =
            [
               new("todo-1", taskDefinition)
            ]
        };
        var workflowDefinitionNode = JsonSerializer.Default.SerializeToNode(workflowDefinition);
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<HttpCallExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.OutputReference.Should().NotBeNull();

        context.Workflow.Instance.Status!.Tasks!.Should().ContainSingle();
        JsonPointer.Parse(context.Workflow.Instance.Status!.Tasks!.First().Reference.OriginalString).TryEvaluate(workflowDefinitionNode, out var match).Should().BeTrue();
        match.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_Using_Basic_Authentication_Should_Work()
    {
        var username = "fake-user";
        var password = "fake-password";
        var parameters = (EquatableDictionary<string, object>)JsonSerializer.Default.Convert(new HttpCallDefinition()
        {
            Method = "get",
            Endpoint = new()
            {
                Uri = new($"https://httpbin.org/basic-auth/{username}/{password}"),
                Authentication = new()
                {
                    Basic = new()
                    {
                        Username = username,
                        Password = password
                    }
                }
            },
            Body = new
            {
                name = "fake-name"
            }
        }, typeof(EquatableDictionary<string, object>))!;
        var definition = new CallTaskDefinition()
        {
            Call = Function.Http,
            With = parameters
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<HttpCallExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = context.Instance.OutputReference == null ? null : JsonSerializer.Default.SerializeToNode((await Documents.GetAsync(context.Instance.OutputReference))!.Content)!.AsObject();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        output.Should().NotBeNull();
        output!.AsObject()!["user"]!.GetValue<string>().Should().Be(username);
    }

}
