using Neuroglia;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Synapse.Runtime.Configuration;
using Synapse.Runtime.Services;

namespace Synapse.UnitTests.Cases.Runtime;

public class NativeRuntimeTests
    : WorkflowRuntimeTestsBase 
{

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<NativeRuntimeOptions>(options => 
        {
            options.Api = new()
            {
                Uri = new("http://localhost")
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
        await process.StartAsync();
        await Task.Delay(100);
        workflowInstance = await this.Resources.GetAsync<WorkflowInstance>(workflowInstance.GetName(), workflowInstance.GetNamespace());

        //assert
        workflow.Should().NotBeNull();
        workflowInstance!.Status.Should().NotBeNull();
        workflowInstance.Status!.Phase.Should().Be(WorkflowInstanceStatusPhase.Completed);
        workflowInstance.Status.OutputReference.Should().BeNullOrWhiteSpace();
        var output = await this.Documents.GetAsync(workflowInstance.Status.OutputReference!);
        output.Should().NotBeNull();
        output!.Content.Should().BeEquivalentTo(input);
    }


}
