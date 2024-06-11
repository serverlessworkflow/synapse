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

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class GrpcCallExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Call_Should_Work()
    {
        //arrange
        var name = "John Doe";
        var parameters = (EquatableDictionary<string, object>)JsonSerializer.Default.Convert(new GrpcCallDefinition()
        {
            Proto = new()
            {
                Uri = new("file:///C:/Users/User/source/repos/GrpcTests/ConsoleApp1/greet.proto")
            },
            Service = new()
            {
                Name = "GreeterApi.Greeter",
                Host = "localhost",
                Port = 5011
            },
            Method = "SayHello",
            Arguments = new Dictionary<string, object>()
            {
                { "name", name }
            }
        }, typeof(EquatableDictionary<string, object>))!;
        var taskDefinition = new CallTaskDefinition()
        {
            Call = Function.Grpc,
            With = parameters
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
        var workflowDefinitionNode = JsonSerializer.Default.SerializeToNode(workflowDefinition);
        var context = await MockTaskExecutionContextFactory.CreateAsync(ServiceProvider, workflowDefinition, taskDefinition, new { });
        var executor = ActivatorUtilities.CreateInstance<GrpcCallExecutor>(ServiceProvider, context);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        context.Instance.OutputReference.Should().NotBeNull();

        context.Workflow.Instance.Status!.Tasks!.Should().ContainSingle();
        JsonPointer.Parse(context.Workflow.Instance.Status!.Tasks!.First().Reference.OriginalString).TryEvaluate(workflowDefinitionNode, out var match).Should().BeTrue();
        match.Should().NotBeNull();
    }

}
