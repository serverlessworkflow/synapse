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

using System.Text.Json;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class SetTaskExecutorTests
    : RunnerTestsBase
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