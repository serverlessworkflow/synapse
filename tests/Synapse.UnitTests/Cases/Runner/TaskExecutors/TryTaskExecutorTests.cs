namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class TryTaskExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Catch_Error_Should_Work()
    {
        //arrange
        var error = new ErrorDefinition()
        {
            Status = ErrorStatus.Runtime,
            Type = ErrorType.Runtime.OriginalString,
            Title = ErrorTitle.Runtime,
            Detail = "fake-detail"
        };
        var definition = new TryTaskDefinition()
        {
            Try = new RaiseTaskDefinition()
            {
                Raise = new()
                {
                    Error = error
                }
            },
            Catch = new()
            {
                Errors = new()
                {
                    With =
                    [
                        new("status", "${ . == 500 }")
                    ]
                },
                As = "err",
                When = "${ $err != null }"
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<TryTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Error.Should().BeNull();
    }

    [Fact]
    public async Task Catch_Error_Should_Fault()
    {
        //arrange
        var error = new ErrorDefinition()
        {
            Status = ErrorStatus.Runtime,
            Type = ErrorType.Runtime.OriginalString,
            Title = ErrorTitle.Runtime,
            Detail = "fake-detail"
        };
        var definition = new TryTaskDefinition()
        {
            Try = new RaiseTaskDefinition()
            {
                Raise = new()
                {
                    Error = error
                }
            },
            Catch = new()
            {
                Errors = new()
                {
                    With =
                    [
                        new("status", "${ . != 500 }")
                    ]
                },
                As = "err",
                When = "${ $err != null }"
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<TryTaskExecutor>(ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Status.Should().Be(ushort.Parse(error.Status.ToString()!));
        context.Instance.Error.Type.OriginalString.Should().Be(error.Type);
        context.Instance.Error.Title.Should().Be(error.Title);
        context.Instance.Error.Detail?.Should().Be(context.Instance.Error.Detail);
        context.Instance.Error.Instance?.OriginalString.Should().Be($"{context.Instance.Reference.OriginalString}/try");
    }

    [Fact]
    public async Task Catch_Error_Should_Retry_N_Times_Then_Fault()
    {
        //arrange
        var retryCount = 5u;
        var error = new ErrorDefinition()
        {
            Status = ErrorStatus.Runtime,
            Type = ErrorType.Runtime.OriginalString,
            Title = ErrorTitle.Runtime,
            Detail = "fake-detail"
        };
        var definition = new TryTaskDefinition()
        {
            Try = new RaiseTaskDefinition()
            {
                Raise = new()
                {
                    Error = error
                }
            },
            Catch = new()
            {
                Errors = new()
                {
                    With =
                    [
                        new("status", $"${{ . == {ErrorStatus.Runtime} }}")
                    ]
                },
                As = "err",
                When = "${ $err != null }",
                Retry = new()
                {
                    Limit = new()
                    {
                        Attempt = new()
                        {
                            Count = retryCount
                        }
                    }
                }
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<TryTaskExecutor>(ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Status.Should().Be(ushort.Parse(error.Status.ToString()!));
        context.Instance.Error.Type.OriginalString.Should().Be(error.Type);
        context.Instance.Error.Title.Should().Be(error.Title);
        context.Instance.Error.Detail?.Should().Be(context.Instance.Error.Detail);
        context.Instance.Error.Instance?.OriginalString.Should().Be($"{context.Instance.Reference.OriginalString}/retry/4");
        context.Instance.Retries.Should().NotBeNull();
        context.Instance.Retries.Should().HaveCount((int)retryCount);
    }

}
