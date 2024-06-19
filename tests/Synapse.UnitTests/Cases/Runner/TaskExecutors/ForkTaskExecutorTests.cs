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

public class ForkTaskExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Execute_Concurrently_Should_Work()
    {
        //arrange
        var red = "red";
        var green = "green";
        var blue = "blue";
        var taskDefinition = new ForkTaskDefinition()
        {
            Fork = new()
            {
                Compete = false,
                Branches =
                [
                    new("red", new SetTaskDefinition()
                    {
                        Set =
                        [
                            new("colors", @$"${{ .colors + [""{red}""] }}")
                        ]
                    }),
                    new("green", new SetTaskDefinition()
                    {
                        Set =
                        [
                            new("colors", @$"${{ .colors + [""{green}""] }}")
                        ]
                    }),
                    new("blue", new SetTaskDefinition()
                    {
                        Set =
                        [
                            new("colors", @$"${{ .colors + [""{blue}""] }}")
                        ]
                    })
                ]
            }
        };
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Namespace = "default",
                Name = "fake",
                Version = "0.1.0"
            },
            Do =
            [
                new("todo-1", taskDefinition)
            ]
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<ForkTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.OutputReference.Should().NotBeNull();
    }

}