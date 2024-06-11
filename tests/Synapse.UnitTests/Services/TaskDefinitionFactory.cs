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

namespace Synapse.UnitTests.Services;

internal static class TaskDefinitionFactory
{

    internal static CallTaskDefinition Call() => new()
    {
        Call = "fakeFunction",
        With = 
        [
            new("param1", "${ .input1 }"),
            new("param2", "${ .input2 }")
        ],
        Input = new()
        {
            Schema = new()
            {
                Format = SchemaFormat.Json,
                Resource = new() { Uri = new("https://unit.tests.com") }
            },
            From = ". + { myProperty: $SCOPE.myProperty }}"
        },
        Output = new()
        {
            To = ".scopeDataProperty"
        },
        Timeout = new()
        {
            After = Duration.FromSeconds(5)
        }
    };

    internal static CompositeTaskDefinition Composite() => new()
    {
        Execute = new()
        {
            Sequentially = 
            [
                new("sub-todo-1", Emit()),
                new("sub-todo-2", Listen())
            ]
        }

    };

    internal static EmitTaskDefinition Emit() => new()
    {
        Emit = new()
        {
            Event = EventDefinitionFactory.Create()
        }
    };

    internal static ForTaskDefinition For()
    {
        return new()
        {
            For = new()
            {
                Each = "fake-item",
                In = "fake-item-source",
                At = "i"
            },
            While = "fake-condition",
            Do = Emit()
        };
    }

    internal static ListenTaskDefinition Listen() => new()
    {
        Listen = new()
        {
            To = new()
            {
                One = EventFilterDefinitionFactory.Create()
            }
        }
    };

    internal static RaiseTaskDefinition Raise() => new()
    {
        Raise = new()
        {
            Error = ErrorDefinitionFactory.Create()
        }
    };

    internal static RunTaskDefinition RunContainer() => new()
    {
        Run = new()
        {
            Container = new()
            {
                Image = "fake-image:latest",
                Command = "fake command -arg1 --arg2",
                Ports = [new(8080,80)],
                Volumes = [new("/fake-source-data", "/fake/output/data")],
                Environment = [new("EnvironmentVariable1", "Fake Value 1")]
            }
        }
    };

    internal static RunTaskDefinition RunScript() => new()
    {
        Run = new()
        {
            Script = new()
            {
                Language = "javascript",
                Code = @"console.log(""Hello, World!"");"
            }
        }
    };

    internal static RunTaskDefinition RunShell() => new()
    {
        Run = new()
        {
            Shell = new()
            {
                Command = "echo \"Hello, World!\"",
                Environment = [new("EnvironmentVariable1", "Fake Value 1")]
            }
        }
    };

    internal static RunTaskDefinition RunWorkflow() => new()
    {
        Run = new()
        {
            Workflow = new()
            {
                Namespace = "fake-namespace",
                Name = "another-fake-workflow"
            }
        }
    };

    internal static SwitchTaskDefinition Switch() => new()
    {
        Switch =
        [
            new("case1", new()
            {  
                When = "fake-condition-1",
                Then = FlowDirective.Continue
            }),
            new("case2", new()
            {
                When = "fake-condition-2",
                Then = FlowDirective.Exit
            }),
            new("default", new()
            {
                Then = FlowDirective.End
            }),
        ]
    };

    internal static TryTaskDefinition Try()
    {
        return new()
        {
            Try = new CompositeTaskDefinition()
            {
                Execute = new()
                {
                    Sequentially =
                    [
                        new("subtask-1", Call()),
                        new("subtask-2", Emit())
                    ]
                }
            },
            Catch = new()
            {
                Errors = new()
                {
                    With = 
                    [
                        new("type", "com.tests.unit.fake.type.v1")
                    ]
                },
                When = "fake-condition",
                Retry = new()
                {
                    When = "fake-condition",
                    Delay = Duration.FromSeconds(3),
                    Backoff = new()
                    {
                        Exponential = new()
                    },
                    Jitter = new()
                    {
                        From = Duration.FromMilliseconds(50),
                        To = Duration.FromSeconds(1)
                    },
                    Limit = new()
                    {
                        Attempt = new()
                        {
                            Count = 10,
                            Duration = Duration.FromSeconds(10)
                        },
                        Duration = Duration.FromMinutes(15)
                    }
                },
                Do = RunWorkflow()
            }
        };
    }

    internal static WaitTaskDefinition Wait() => new()
    {
        Wait = Duration.FromSeconds(2)
    };

}
