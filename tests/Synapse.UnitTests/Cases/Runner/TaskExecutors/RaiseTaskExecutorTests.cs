namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class RaiseTaskExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Raise_Static_Error_Should_Work()
    {
        //arrange
        var error = new ErrorDefinition()
        {
            Status = ErrorStatus.Runtime,
            Type = ErrorType.Runtime.OriginalString,
            Title = ErrorTitle.Runtime,
            Detail = "fake-detail"
        };
        var definition = new RaiseTaskDefinition()
        {
            Raise = new()
            {
                Error = error
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<RaiseTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Status.Should().Be(ushort.Parse(error.Status.ToString()!));
        context.Instance.Error.Type.OriginalString.Should().Be(error.Type);
        context.Instance.Error.Title.Should().Be(error.Title);
        context.Instance.Error.Detail?.Should().Be(context.Instance.Error.Detail);
        context.Instance.Error.Instance.Should().Be(context.Instance.Reference);
    }

}
