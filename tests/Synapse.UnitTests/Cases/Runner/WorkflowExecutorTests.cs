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

using ServerlessWorkflow.Sdk.IO;
using Synapse.Runner.Services;
using System.Text;

namespace Synapse.UnitTests.Cases.Runner;

public class WorkflowExecutorTests
    : RunnerTestsBase
{

    [Fact]
    public async Task Run_Workflow_Should_Work_Async()
    {
        //arrange
        var yaml = @"
document:
  dsl: '0.1'
  namespace: default
  name: hello-chain
  version: '0.2.4'
do:
- say-hello-1:
    set:
      greetings: ${ ""Hello "" + (. // ""world"") }
    input:
      from: ${ $workflow.input.name1 }
    export:
      as:
        greetings: ${ $output.greetings }
- say-hello-2:
    set:
      greetings: ${ $context.greetings + "" and "" + (. // ""world"") }
    input:
      from: ${ $workflow.input.name2 }
";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
        var definition = await this.ServiceProvider.GetRequiredService<IWorkflowDefinitionReader>()
            .ReadAsync(stream);
        var input = new Neuroglia.EquatableDictionary<string, object>()
        {
            new("name1", "John Doe"),
            new("name2", "Jane Doe")
        };
        var context = await MockWorkflowExecutionContextFactory.CreateAsync(this.ServiceProvider, definition, input);
        var executor = ActivatorUtilities.CreateInstance<WorkflowExecutor>(ServiceProvider, context);

        //act
        await executor.ExecuteAsync();

        //assert
        context.Output.Should().NotBeNull();
    }

}
