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

namespace Synapse.UnitTests.Cases.Runner.TaskExecutors;

public class EmitTaskExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Emit_Should_Work()
    {
        //arrange
        var id = Guid.NewGuid().ToString();
        var source = "https://events.fake-source.com";
        var type = "com.fake.event.type.v1";
        var data = new
        {
            foo = new
            {
                bar = "baz"
            }
        };
        var taskDefinition = new EmitTaskDefinition()
        {
            Emit = new()
            {
                Event = new()
                {
                    With =
                    [
                        new(nameof(id), id),
                        new(nameof(source), source),
                        new(nameof(type), type),
                        new(nameof(data), "${ .fooBar }")
                    ]
                }
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
        var workflowDefinitionNode = JsonSerializer.Default.SerializeToNode(workflowDefinition);
        var input = new Dictionary<string, object>() { { "fooBar", data } };
        var context = await MockTaskExecutionContextFactory.CreateAsync(this.ServiceProvider, workflowDefinition, taskDefinition, input);
        var executor = ActivatorUtilities.CreateInstance<EmitTaskExecutor>(this.ServiceProvider, context);
        var events = new List<CloudEvent>();
        this.ServiceProvider.GetRequiredService<ICloudEventBus>().OutputStream.Subscribe(events.Add);

        //act
        await executor.InitializeAsync();
        await executor.ExecuteAsync();

        //assert
        context.Instance.Status.Should().Be(TaskInstanceStatus.Completed);
        events.Should().NotBeNullOrEmpty();
        events.Should().HaveCount(1);
        var e = events.First();
        e.Id.Should().Be(id);
        e.Source.OriginalString.Should().Be(source);
        e.Type.Should().Be(type);
        JsonSerializer.Default.Convert(e.Data, typeof(IDictionary<string, object>)).Should().BeEquivalentTo(JsonSerializer.Default.Convert(data, typeof(IDictionary<string, object>)));
        context.Workflow.Instance.Status!.Tasks!.Should().ContainSingle();
        JsonPointer.Parse(context.Workflow.Instance.Status!.Tasks!.First().Reference.OriginalString).TryEvaluate(workflowDefinitionNode, out var match).Should().BeTrue();
        match.Should().NotBeNull();
    }

}