using System.Text.Json;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class SetTaskExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Set_Should_Work()
    {
        //arrange
        var colors = new string[] { "red", "green", "blue" };
        var definition = new SetTaskDefinition()
        {
            Set =
            [
                new(nameof(colors), colors),
            ]
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<SetTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = context.Instance.OutputReference == null ? null : Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await Documents.GetAsync(context.Instance.OutputReference))!.Content)?.AsObject();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        var outputColors = output?[nameof(colors)].Deserialize<string[]>();
        outputColors.Should().NotBeNull();
        outputColors.Should().BeEquivalentTo(colors);
    }

}