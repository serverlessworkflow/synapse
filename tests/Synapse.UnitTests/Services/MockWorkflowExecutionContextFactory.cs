using Synapse.Runner.Services;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;

namespace Synapse.UnitTests.Services;

internal static class MockWorkflowExecutionContextFactory
{

    internal static IWorkflowExecutionContext Create(IServiceProvider services, WorkflowDefinition definition, WorkflowInstance instance) => ActivatorUtilities.CreateInstance<WorkflowExecutionContext>(services, definition, instance);

    internal static async Task<IWorkflowExecutionContext> CreateAsync(IServiceProvider services, WorkflowDefinition? workflowDefinition = null) 
    {
        var resources = services.GetRequiredService<IResourceRepository>();
        workflowDefinition ??= WorkflowDefinitionFactory.Create();
        var workflow = await resources.AddAsync(new Workflow() 
        { 
            Metadata = new()
            {
                Name = workflowDefinition.Document.Name,
                Namespace = workflowDefinition.Document.Namespace
            },
            Spec = new()
            {
                Versions = [ workflowDefinition ]
            }
        });
        var workflowInstance = await resources.AddAsync(new WorkflowInstance()
        {
            Metadata = new()
            {
                Name = $"{workflow.GetName()}-{Guid.NewGuid().ToString("N")[..15]}",
                Namespace = workflow.GetNamespace()
            },
            Spec = new()
            {
                Definition = new()
                {
                    Name = workflow.GetName(),
                    Namespace = workflow.GetNamespace()!,
                    Version = workflowDefinition.Document.Version
                },
                Input = []
            },
            Status = new()
            {

            }
        });
        return Create(services, workflowDefinition, workflowInstance);
    }

}