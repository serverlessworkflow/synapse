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

using System.Net;
using System.Text.Json;

namespace Synapse.UnitTests.Cases.Runner;

public class TaskExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Task_Should_Timeout()
    {
        //arrange
        var definition = new RunTaskDefinition()
        {
            Run = new()
            {
                Shell = new()
                {
                    Command = "echo Hello, World!"
                }
            },
            Timeout = new()
            {
                After = Duration.FromMilliseconds(1)
            }
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<ShellProcessExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = context.Instance.OutputReference == null ? null : (await Documents.GetAsync(context.Instance.OutputReference))!.Content;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Runs.Should().NotBeNullOrEmpty();
        var run = context.Instance.Runs!.FirstOrDefault();
        run.Should().NotBeNull();
        run!.EndedAt.Should().HaveValue();
        run.Outcome.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Status.Should().Be((int)HttpStatusCode.RequestTimeout);
        context.Instance.Error!.Type.Should().Be(ErrorType.Timeout);
        context.Instance.Error!.Title.Should().Be(ErrorTitle.Timeout);
        output.Should().BeNull();
    }

    [Fact]
    public async Task Filter_Task_Input_And_Output_Should_Work()
    {
        //arrange
        var colors = new string[] { "red", "green", "blue" };
        var shapes = new string[] { "circle", "square" };
        var sizes = new string[] { "small", "normal", "large" };
        var fonts = new string[] { "arial", "comic-sans-ms" };
        var originalInput = new { resources = new { fonts } };
        var definition = new SetTaskDefinition()
        {
            Set =
            [
                new(nameof(colors), colors),
                new(nameof(shapes), shapes),
                new(nameof(sizes), sizes)
            ],
            Input = new()
            {
                From = "{ fonts: .resources.fonts }"
            },
            Output = new()
            {
                From = "{ fonts: $input.fonts, sizes }"
            }
        };
        var workflow = await MockWorkflowExecutionContextFactory.CreateAsync(ServiceProvider);
        var task = await workflow.CreateTaskAsync(definition, "/fake", originalInput);
        var input = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await Documents.GetAsync(task.InputReference!))!.Content)!.AsObject()!;
        var context = MockTaskExecutionContextFactory.Create(workflow, task, definition, input);
        var executor = ActivatorUtilities.CreateInstance<SetTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await Documents.GetAsync(context.Instance.OutputReference!))!.Content)!.AsObject()!;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        input.Count.Should().Be(1);
        input[nameof(fonts)]?.Deserialize<string[]>().Should().BeEquivalentTo(originalInput.resources.fonts);
        output.Count.Should().Be(2);
        output[nameof(fonts)].Deserialize<string[]>().Should().BeEquivalentTo(fonts);
        output[nameof(sizes)].Deserialize<string[]>().Should().BeEquivalentTo(sizes);
    }

}
