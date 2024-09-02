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

public class WaitTaskExecutorTests
    : RunnerTestsBase
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
