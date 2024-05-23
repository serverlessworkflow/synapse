namespace Synapse.UnitTests.Cases.Worker.TaskExecutors;

public class ProcessTaskExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Run_Container_Should_Work()
    {
        //arrange
        var greetings = "Hello, World!";
        var definition = new RunTaskDefinition()
        {
            Run = new()
            {
                Container = new()
                {
                    Image = "alpine:latest",
                    Command = $"echo {greetings}"
                }
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<ContainerProcessExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = context.Instance.OutputReference == null ? null : (await Documents.GetAsync(context.Instance.OutputReference))!.Content;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        output.Should().Be(greetings);
    }

    [Fact]
    public async Task Run_Shell_Should_Work()
    {
        //arrange
        var greetings = "Hello, World!";
        var definition = new RunTaskDefinition()
        {
            Run = new()
            {
                Shell = new()
                {
                    Command = $"echo {greetings}"
                }
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<ShellProcessExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = context.Instance.OutputReference == null ? null : (await Documents.GetAsync(context.Instance.OutputReference))!.Content;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        output.Should().Be(greetings);
    }

}
