using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.UnitTests.Cases.Worker;

public class ExtensionTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Run_Before_All_Should_Work()
    {
        //arrange
        var variableName = "foo";
        var valueBefore = "bar";
        var valueAfter = "baz";
        var extension = new ExtensionDefinition()
        {
            Extend = "all",
            Before = new SetTaskDefinition()
            {
                Set =
                [
                    new(variableName, valueBefore)
                ],
                Then = FlowDirective.Exit
            }
        };
        var taskDefinition = new SetTaskDefinition()
        {
            Set =
            [
                new(variableName, valueAfter)
            ]
        };
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Namespace = "default",
                Name = "test",
                Version = "0.1.0"
            },
            Use = new()
            {
                Extensions =
                [
                    new("intercept", extension)
                ]
            },
            Do =
            [
                new("todo-1", taskDefinition)
            ]
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<SetTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Error.Should().BeNull();
        var output = (await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content.ConvertTo<Dictionary<string, object>>()!;
        output.Should().NotBeNull();
        output[variableName] = valueBefore;
    }

    [Fact]
    public async Task Run_After_All_Should_Work()
    {
        //arrange
        var variableName = "foo";
        var valueBefore = "bar";
        var valueAfter = "baz";
        var extension = new ExtensionDefinition()
        {
            Extend = "all",
            After = new SetTaskDefinition()
            {
                Set =
                [
                    new(variableName, valueAfter)
                ],
                Then = FlowDirective.Exit
            }
        };
        var taskDefinition = new SetTaskDefinition()
        {
            Set =
            [
                new(variableName, valueBefore)
            ]
        };
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Namespace = "default",
                Name = "test",
                Version = "0.1.0"
            },
            Use = new()
            {
                Extensions =
                [
                    new("intercept", extension)
                ]
            },
            Do =
            [
                new("todo-1", taskDefinition)
            ]
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<SetTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Error.Should().BeNull();
        var output = (await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content.ConvertTo<Dictionary<string, object>>()!;
        output.Should().NotBeNull();
        output[variableName] = valueAfter;
    }

}
