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

using Neuroglia;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Runtime.Services;

namespace Synapse.UnitTests.Cases.Runtime;

public class NativeRuntimeTests
    : WorkflowRuntimeTestsBase 
{

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<RunnerDefinition>(options => 
        {
            options.Api = new()
            {
                Uri = new("http://localhost")
            };
            options.Runtime.Native = new()
            {
                Directory = Path.Combine("..", "..", "..", "..", "..", "src", "runner", "Synapse.Runner", "bin", "Debug", "net8.0")
            };
        });
        services.AddSingleton<IWorkflowRuntime, NativeRuntime>();
    }

    [Fact]
    public async Task Create_Process_Should_Work()
    {
        //arrange
        var input = new EquatableDictionary<string, object>()
        {
            new("red", 69),
            new("green", 69),
            new("blue", 69)
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
            Do = 
            [
                new("setInput", new SetTaskDefinition()
                {
                    Set = input
                })
            ]
        };
        var workflow = await this.Resources.AddAsync(new Workflow()
        {
            Metadata = new()
            {
                Namespace = workflowDefinition.Document.Namespace,
                Name = workflowDefinition.Document.Name
            },
            Spec = new()
            {
                Versions = [ workflowDefinition ]
            }
        });
        var workflowInstance = await this.Resources.AddAsync(new WorkflowInstance()
        {
            Metadata = new()
            {
                Namespace = "default",
                Name = "test-"
            },
            Spec = new()
            {
                Definition = new()
                {
                    Namespace = workflow.GetNamespace()!,
                    Name = workflow.GetName(),
                    Version = workflowDefinition.Document.Version
                },
                Input = input
            }
        });

        //act
        var process = await this.Runtime.CreateProcessAsync(workflow, workflowInstance);
       
        //assert
        process.Should().NotBeNull();
        var startTask = () => process.StartAsync();
        await startTask.Should().NotThrowAsync();

        // We cannot assert much more here because the runner needs a working API, and will therefore fail to run.
        // Further tests should be made in the Integration Testing project.
    }


}
