namespace Synapse.UnitTests.Cases.Worker.TaskExecutors;

public class SwitchTaskExecutorTests
    : WorkerTestsBase
{

    [Fact]
    public async Task Switch_Match_Single_Case_Should_Work()
    {
        //arrange
        var color = "color";
        var red = "red";
        var green = "green";
        var blue = "blue";
        var definition = new SwitchTaskDefinition()
        {
            Switch =
            [
                new(red, new()
                {
                    When = @$".{color} == ""{red}""",
                    Then = red
                }),
                new(green, new()
                {
                    When = @$".{color} == ""{green}""",
                    Then = green
                }),
                new(blue, new()
                {
                    When = @$".{color} == ""{blue}""",
                    Then = blue
                })
            ]
        };
        var input = new Dictionary<string, object>() { { color, green } };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, input);
        var executor = ActivatorUtilities.CreateInstance<SwitchTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Next.Should().Be(green);
    }

    [Fact]
    public async Task Switch_Match_Fallback_To_Default_Should_Work()
    {
        //arrange
        var color = "color";
        var red = "red";
        var green = "green";
        var blue = "blue";
        var other = "other";
        var definition = new SwitchTaskDefinition()
        {
            Switch =
            [
                new(red, new()
                {
                    When = @$".{color} == ""{red}""",
                    Then = red
                }),
                new(green, new()
                {
                    When = @$".{color} == ""{green}""",
                    Then = green
                }),
                new(blue, new()
                {
                    When = @$".{color} == ""{blue}""",
                    Then = blue
                }),
                new(other, new()
                {
                    Then = other
                })
            ]
        };
        var input = new Dictionary<string, object>() { { color, "magenta" } };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, input);
        var executor = ActivatorUtilities.CreateInstance<SwitchTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Next.Should().Be(other);
    }

    [Fact]
    public async Task Switch_Match_Multiple_Cases_Should_Fault()
    {
        //arrange
        var color = "color";
        var red = "red";
        var green = "green";
        var blue = "blue";
        var definition = new SwitchTaskDefinition()
        {
            Switch =
            [
                new(red, new()
                {
                    When = @$".{color} == ""{red}""",
                    Then = red
                }),
                new(green, new()
                {
                    When = @$".{color} == ""{red}""",
                    Then = green
                }),
                new(blue, new()
                {
                    When = @$".{color} == ""{red}""",
                    Then = blue
                })
            ]
        };
        var input = new Dictionary<string, object>() { { color, red } };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, input);
        var executor = ActivatorUtilities.CreateInstance<SwitchTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Type.Should().Be(ErrorType.Configuration);
        context.Instance.Error!.Status.Should().Be(ErrorStatus.Configuration);
    }

}
