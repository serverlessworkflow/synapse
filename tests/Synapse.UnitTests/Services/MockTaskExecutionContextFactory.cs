using Synapse.Worker.Application.Services;

namespace Synapse.UnitTests.Services;

internal static class MockTaskExecutionContextFactory
{

    internal static ITaskExecutionContext<TDefinition> Create<TDefinition>(IWorkflowExecutionContext workflow, TaskInstance instance, TDefinition definition, object input)
    where TDefinition : TaskDefinition
    {
        return new TaskExecutionContext<TDefinition>(workflow, instance, definition, input, new Dictionary<string, object>(), new Dictionary<string, object>());
    }

    internal static async Task<ITaskExecutionContext<TDefinition>> CreateAsync<TDefinition>(IServiceProvider serviceProvider, WorkflowDefinition? workflowDefinition, TDefinition taskDefinition, object input)
    where TDefinition : TaskDefinition
    {
        workflowDefinition ??= new WorkflowDefinition()
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
        var workflowExecutionContext = await MockWorkflowExecutionContextFactory.CreateAsync(serviceProvider, workflowDefinition);
        var instance = await workflowExecutionContext.CreateTaskAsync(taskDefinition, workflowDefinition?.Do.First().Key!, input);
        return Create(workflowExecutionContext, instance, taskDefinition, input);
    }

    internal static Task<ITaskExecutionContext<TDefinition>> CreateAsync<TDefinition>(IServiceProvider serviceProvider, TDefinition taskDefinition, object input)
        where TDefinition : TaskDefinition
    {
        return CreateAsync(serviceProvider, null, taskDefinition, input);
    }

}