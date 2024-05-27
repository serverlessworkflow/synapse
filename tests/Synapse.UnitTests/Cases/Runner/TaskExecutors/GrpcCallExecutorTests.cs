using Json.Pointer;
using Neuroglia;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class GrpcCallExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Call_Should_Work()
    {
        //arrange
        var name = "John Doe";
        var parameters = (EquatableDictionary<string, object>)JsonSerializer.Default.Convert(new GrpcCallDefinition()
        {
            Proto = new()
            {
                Uri = new("file:///C:/Users/User/source/repos/GrpcTests/ConsoleApp1/greet.proto")
            },
            Service = new()
            {
                Name = "GreeterApi.Greeter",
                Host = "localhost",
                Port = 5011
            },
            Method = "SayHello",
            Arguments = new Dictionary<string, object>()
            {
                { "name", name }
            }
        }, typeof(EquatableDictionary<string, object>))!;
        var taskDefinition = new CallTaskDefinition()
        {
            Call = Function.Grpc,
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
        var executor = ActivatorUtilities.CreateInstance<GrpcCallExecutor>(ServiceProvider, context);

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

}
