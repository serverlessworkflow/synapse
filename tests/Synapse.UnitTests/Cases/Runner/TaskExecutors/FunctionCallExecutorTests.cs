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

using Json.Schema;
using ServerlessWorkflow.Sdk.Builders;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class FunctionCallExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Call_Function_Should_Work_Async()
    {
        //arrange
        var username = "john.doe@email.com";
        var functionName = "testFunction";
        var functionDefinition = new SetTaskDefinition()
        {
            Set = 
            [
                new("user", new
                {  
                    Name = "${ .username }"
                })
            ],
            Input = new()
            {
                Schema = new()
                {
                    Format = SchemaFormat.Json,
                    Document = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Properties(("username", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)))
                        .Required("username")
                        .Build()
                }
            }
        };
        var parameters = new Neuroglia.EquatableDictionary<string, object>()
        {
            new("username", "${ .user.username }")
        };
        var taskDefinition = new CallTaskDefinition()
        {
            Call = functionName,
            With = parameters
        };
        var workflowDefinition = new WorkflowDefinitionBuilder()
            .WithNamespace("default")
            .WithName("fake-workflow")
            .WithVersion("1.0.0")
            .UseFunction(functionName, functionDefinition)
            .Do("callTestFunction", taskDefinition)
            .Build();
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, workflowDefinition, taskDefinition, new
        {
            user = new
            {
                username
            }
        });
        var executor = ActivatorUtilities.CreateInstance<FunctionCallExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.OutputReference.Should().NotBeNull();

    }

    [Fact]
    public async Task Call_Function_WithInvalidInput_Should_Fault_Async()
    {
        //arrange
        var username = "john.doe@email.com";
        var functionName = "testFunction";
        var functionDefinition = new SetTaskDefinition()
        {
            Set =
            [
                new("user", new
                {
                    Name = "${ .username }"
                })
            ],
        };
        var parameters = new Neuroglia.EquatableDictionary<string, object>()
        {
            new("foo", "${ .user.username }")
        };
        var taskDefinition = new CallTaskDefinition()
        {
            Call = functionName,
            With = parameters,
            Input = new()
            {
                Schema = new()
                {
                    Format = SchemaFormat.Json,
                    Document = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Properties(("username", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)))
                        .Required("username")
                        .Build()
                }
            }
        };
        var workflowDefinition = new WorkflowDefinitionBuilder()
            .WithNamespace("default")
            .WithName("fake-workflow")
            .WithVersion("1.0.0")
            .UseFunction(functionName, functionDefinition)
            .Do("callTestFunction", taskDefinition)
            .Build();
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, workflowDefinition, taskDefinition, new
        {
            user = new
            {
                username
            }
        });
        var executor = ActivatorUtilities.CreateInstance<FunctionCallExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Faulted);
        context.Instance.Error.Should().NotBeNull();
        context.Instance.Error!.Type.Should().Be(ErrorType.Validation);
        context.Instance.Error!.Status.Should().Be(ErrorStatus.Validation);
        context.Instance.Error!.Title.Should().Be(ErrorTitle.Validation);
    }

}
