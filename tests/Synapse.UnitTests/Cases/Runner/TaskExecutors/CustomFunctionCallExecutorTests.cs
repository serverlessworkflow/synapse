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

using ServerlessWorkflow.Sdk.Builders;

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class CustomFunctionCallExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Call_CustomFunction_Should_Work()
    {
        //arrange
        var username = "John Doe";
        var parameters = new Neuroglia.EquatableDictionary<string, object>()
        {
            new("message", "Hello, {user}!"),
            new("arguments", new { user = username })
        };
        var functionName = "https://raw.githubusercontent.com/serverlessworkflow/catalog/main/functions/log/1.0.0/function.yaml";
        var taskDefinition = new CallTaskDefinition()
        {
            Call = functionName,
            With = parameters
        };
        var workflowDefinition = new WorkflowDefinitionBuilder()
            .WithNamespace("default")
            .WithName("fake-workflow")
            .WithVersion("1.0.0")
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

}
