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

using Neuroglia.Data.Infrastructure.ResourceOriented;

namespace Synapse.UnitTests.Cases.Runner;

public class ExtensionTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Run_Before_All_Should_Work()
    {
        //arrange
        var variableName = "foo";
        var valueBefore = "bar";
        var valueAfter = "baz";
        var extension = new ExtensionDefinition()
        {
            Extend = "all",
            Before = new SetTaskDefinition()
            {
                Set =
                [
                    new(variableName, valueBefore)
                ],
                Then = FlowDirective.Exit
            }
        };
        var taskDefinition = new SetTaskDefinition()
        {
            Set =
            [
                new(variableName, valueAfter)
            ]
        };
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Namespace = "default",
                Name = "test",
                Version = "0.1.0"
            },
            Use = new()
            {
                Extensions =
                [
                    new("intercept", extension)
                ]
            },
            Do =
            [
                new("todo-1", taskDefinition)
            ]
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<SetTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Error.Should().BeNull();
        var output = (await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content.ConvertTo<Dictionary<string, object>>()!;
        output.Should().NotBeNull();
        output[variableName] = valueBefore;
    }

    [Fact]
    public async Task Run_After_All_Should_Work()
    {
        //arrange
        var variableName = "foo";
        var valueBefore = "bar";
        var valueAfter = "baz";
        var extension = new ExtensionDefinition()
        {
            Extend = "all",
            After = new SetTaskDefinition()
            {
                Set =
                [
                    new(variableName, valueAfter)
                ],
                Then = FlowDirective.Exit
            }
        };
        var taskDefinition = new SetTaskDefinition()
        {
            Set =
            [
                new(variableName, valueBefore)
            ]
        };
        var workflowDefinition = new WorkflowDefinition()
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Namespace = "default",
                Name = "test",
                Version = "0.1.0"
            },
            Use = new()
            {
                Extensions =
                [
                    new("intercept", extension)
                ]
            },
            Do =
            [
                new("todo-1", taskDefinition)
            ]
        };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<SetTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Error.Should().BeNull();
        var output = (await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content.ConvertTo<Dictionary<string, object>>()!;
        output.Should().NotBeNull();
        output[variableName] = valueAfter;
    }

}
