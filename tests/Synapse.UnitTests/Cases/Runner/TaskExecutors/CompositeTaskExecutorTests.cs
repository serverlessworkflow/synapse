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
using System.Text.Json;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class CompositeTaskExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Execute_Sequentially_Should_Work()
    {
        //arrange
        var red = "red";
        var green = "green";
        var blue = "blue";
        var taskDefinition = new CompositeTaskDefinition()
        {
            Execute = new()
            {
                Sequentially =
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
        var workflowDefinitionNode = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode(workflowDefinition);
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<SequentialCompositeTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.OutputReference.Should().NotBeNull();
        var document = (await Documents.GetAsync(context.Instance.OutputReference!))!;
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode(document.Content)!.AsObject();
        output.Should().NotBeNull();

        var colors = output!["colors"]!.Deserialize<string[]>()!;
        colors.Should().HaveCount(3);
        colors[0].Should().Be(red);
        colors[1].Should().Be(green);
        colors[2].Should().Be(blue);

        foreach (var task in context.Workflow.Instance.Status!.Tasks!)
        {
            var pointer = JsonPointer.Parse(task.Reference.OriginalString);
            pointer.TryEvaluate(workflowDefinitionNode, out var match).Should().BeTrue();
            match.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Execute_Sequentially_Until_Exit_Directive_Should_Work()
    {
        //arrange
        var red = "red";
        var green = "green";
        var blue = "blue";
        var definition = new CompositeTaskDefinition()
        {
            Execute = new()
            {
                Sequentially =
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
                        ],
                        Then = FlowDirective.Exit
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
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<SequentialCompositeTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Next.Should().Be(FlowDirective.Continue);
        context.Instance.OutputReference.Should().NotBeNull();
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await Documents.GetAsync(context.Instance.OutputReference!))!.Content)!.AsObject();

        var colors = output!["colors"]!.Deserialize<string[]>()!;
        colors.Should().HaveCount(2);
        colors[0].Should().Be(red);
        colors[1].Should().Be(green);
    }

    [Fact]
    public async Task Execute_Sequentially_Until_End_Directive_Should_Work()
    {
        //arrange
        var red = "red";
        var green = "green";
        var blue = "blue";
        var definition = new CompositeTaskDefinition()
        {
            Execute = new()
            {
                Sequentially =
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
                        ],
                        Then = FlowDirective.End
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
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<SequentialCompositeTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.Next.Should().Be(FlowDirective.End);
        context.Instance.OutputReference.Should().NotBeNull();
        var output = Neuroglia.Serialization.Json.JsonSerializer.Default.SerializeToNode((await Documents.GetAsync(context.Instance.OutputReference!))!.Content)!.AsObject();
        var colors = output!["colors"]!.Deserialize<string[]>()!;
        colors.Should().HaveCount(2);
        colors[0].Should().Be(red);
        colors[1].Should().Be(green);
    }

    [Fact]
    public async Task Execute_Sequentially_Faulted_SubTask_Should_Fault()
    {
        //arrange
        var error = new ErrorDefinition()
        {
            Status = ErrorStatus.Configuration,
            Type = ErrorType.Configuration.OriginalString,
            Title = ErrorTitle.Configuration
        };
        var red = "red";
        var blue = "blue";
        var definition = new CompositeTaskDefinition()
        {
            Execute = new()
            {
                Sequentially =
                [
                    new("red", new SetTaskDefinition()
                    {
                        Set =
                        [
                            new("colors", @$"${{ .colors + [""{red}""] }}")
                        ]
                    }),
                    new("green", new RaiseTaskDefinition()
                    {
                        Raise = new()
                        {
                            Error = error
                        }
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
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, definition, new { });
        var executor = ActivatorUtilities.CreateInstance<SequentialCompositeTaskExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Status.ToString().Should().Be(error.Status.ToString());
        context.Instance.Error!.Type.OriginalString.Should().Be(error.Type);
        context.Instance.Error!.Title.Should().Be(error.Title);
    }

    [Fact]
    public async Task Execute_Concurrently_Should_Work()
    {
        //arrange
        var red = "red";
        var green = "green";
        var blue = "blue";
        var taskDefinition = new CompositeTaskDefinition()
        {
            Execute = new()
            {
                Concurrently =
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
        var executor = ActivatorUtilities.CreateInstance<ConcurrentCompositeTaskExecutor>(this.ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.OutputReference.Should().NotBeNull();
    }

}
