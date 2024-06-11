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

using Json.Pointer;
using Neuroglia;
using System.Text.Json;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class ForTaskExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Iterate_Should_Work()
    {
        //arrange
        var indexes = new int[] { 0, 1, 2 };
        var colors = new string[] { "red", "green", "blue" };
        var taskDefinition = new ForTaskDefinition()
        {
            For = new()
            {
                Each = "color",
                In = ".colors"
            },
            Do = new SetTaskDefinition()
            {
                Set =
                [
                    new("output", "${ { colors: (.output.colors + [ $color ]), indexes: (.output.indexes + [ $index ])} }")
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
        var workflowDefinitionNode = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode(workflowDefinition)!;
        var input = new { colors };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, workflowDefinition, taskDefinition, input);
        var executor = ActivatorUtilities.CreateInstance<ForTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content)!.AsObject()!;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        output.Count.Should().Be(1);
        output.TryGetPropertyValue("output", out var result).Should().BeTrue();
        result!.AsObject()!.Count.Should().Be(2);
        result!.AsObject()!.TryGetPropertyValue(nameof(colors).ToCamelCase(), out var outputColors).Should().BeTrue();
        outputColors!.Deserialize<string[]>().Should().BeEquivalentTo(colors);
        result!.AsObject()!.TryGetPropertyValue(nameof(indexes).ToCamelCase(), out var outputIndexes).Should().BeTrue();
        outputIndexes!.Deserialize<int[]>().Should().BeEquivalentTo(indexes);

        var index = 0;
        JsonPointer.Parse(context.Workflow.Instance.Status!.Tasks!.First().Reference.OriginalString).TryEvaluate(workflowDefinitionNode, out var match).Should().BeTrue();
        match.Should().NotBeNull();
        foreach (var task in context.Workflow.Instance.Status!.Tasks!.Skip(1))
        {
            task.Reference.OriginalString.Should().EndWith($"{index}/do");
            index++;
        }
    }

    [Fact]
    public async Task Iterate_Until_Exit_Directive_Should_Work()
    {
        var letters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        var lastIndex = 11;
        var definition = new ForTaskDefinition()
        {
            For = new()
            {
                Each = "letter",
                In = ".letters"
            },
            Do = new CompositeTaskDefinition()
            {
                Execute = new()
                {
                    Sequentially = 
                    [
                        new("checkWhetherBeforeOrAfterLastIndex", new SwitchTaskDefinition()
                        {
                            Switch =
                            [
                                new("afterL", new()
                                {
                                    When = $"$index > {lastIndex}",
                                    Then = FlowDirective.Exit
                                })
                            ]
                        }),
                        new("setLastIndex", new SetTaskDefinition()
                        {
                            Set =
                            [
                                new("lastIndex", "${ $index }")
                            ]
                        })
                    ]
                }
            }
        };
        var input = new { letters };
        var workflow = await MockWorkflowExecutionContextFactory.CreateAsync(this.ServiceProvider);
        var task = await workflow.CreateTaskAsync(definition, "fake", input);
        var context = MockTaskExecutionContextFactory.Create(workflow, task, definition, input);
        var executor = ActivatorUtilities.CreateInstance<ForTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content)!.AsObject()!;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Next.Should().Be(FlowDirective.Continue);
        output.Count.Should().Be(1);
        output["lastIndex"].Deserialize<int>().Should().Be(lastIndex);
    }

    [Fact]
    public async Task Iterate_Until_End_Directive_Should_Work()
    {
        var letters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        var lastIndex = 9;
        var definition = new ForTaskDefinition()
        {
            For = new()
            {
                Each = "letter",
                In = ".letters"
            },
            Do = new CompositeTaskDefinition()
            {
                Execute = new()
                {
                    Sequentially =
                    [
                        new("checkWhetherBeforeOrAfterLastIndex", new SwitchTaskDefinition()
                        {
                            Switch =
                            [
                                new("afterL", new()
                                {
                                    When = $"$index > {lastIndex}",
                                    Then = FlowDirective.End
                                })
                            ]
                        }),
                        new("setLastIndex", new SetTaskDefinition()
                        {
                            Set =
                            [
                                new("lastIndex", "${ $index }")
                            ]
                        })
                    ]
                }
            }
        };
        var input = new { letters };
        var workflow = await MockWorkflowExecutionContextFactory.CreateAsync(this.ServiceProvider);
        var task = await workflow.CreateTaskAsync(definition, "fake", input);
        var context = MockTaskExecutionContextFactory.Create(workflow, task, definition, input);
        var executor = ActivatorUtilities.CreateInstance<ForTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await this.Documents.GetAsync(context.Instance.OutputReference!))!.Content)!.AsObject()!;

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Next.Should().Be(FlowDirective.End);
        output.Count.Should().Be(1);
        output["lastIndex"].Deserialize<int>().Should().Be(lastIndex);
    }

}
