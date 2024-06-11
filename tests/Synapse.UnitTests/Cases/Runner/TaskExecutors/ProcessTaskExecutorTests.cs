// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class ProcessTaskExecutorTests
    : RunnerTestsBase
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
