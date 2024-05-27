namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class WaitTaskExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Wait_Should_Work()
    {
        //arrange
        var duration = Duration.FromMilliseconds(150);
        var definition = new WaitTaskDefinition()
        {
            Wait = duration
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<WaitTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Runs.Should().NotBeNullOrEmpty();
        var run = context.Instance.Runs!.First();
        run.EndedAt.HasValue.Should().BeTrue();
        (run.EndedAt!.Value - run.StartedAt).TotalMilliseconds.Should().BeGreaterThanOrEqualTo(duration.TotalMilliseconds);
    }

}
