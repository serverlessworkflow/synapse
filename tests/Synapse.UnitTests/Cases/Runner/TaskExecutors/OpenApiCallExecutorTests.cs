using Json.Pointer;
using Neuroglia;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class OpenApiCallExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task GetPetById_Should_Work()
    {
        //arrange
        var status = "available";
        var parameters = (EquatableDictionary<string, object>)JsonSerializer.Default.Convert(new OpenApiCallDefinition()
        {
            Document = new() { Uri = new("https://petstore.swagger.io/v2/swagger.json") },
            OperationId = "findPetsByStatus",
            Parameters =
            [
                new(nameof(status), status)
            ]
        }, typeof(EquatableDictionary<string, object>))!;
        var taskDefinition = new CallTaskDefinition()
        {
            Call = Function.OpenApi,
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
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<OpenApiCallExecutor>(ServiceProvider, context);

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
