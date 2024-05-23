using ServerlessWorkflow.Sdk;
using Neuroglia;

namespace Synapse.UnitTests.Services;

internal static class WorkflowDefinitionFactory
{

    internal static WorkflowDefinition Create()
    {
        return new WorkflowDefinition
        {
            Document = new()
            {
                Dsl = DslVersion.V010,
                Name = "fake",
                Version = "1.0.0",
            },
            Do = new EquatableDictionary<string, TaskDefinition>
            ([
                new("todo-1", TaskDefinitionFactory.Call()),
                new("todo-2", TaskDefinitionFactory.Composite()),
                new("todo-3", TaskDefinitionFactory.Emit()),
                new("todo-4", TaskDefinitionFactory.For()),
                new("todo-5", TaskDefinitionFactory.Listen()),
                new("todo-6", TaskDefinitionFactory.Raise()),
                new("todo-7", TaskDefinitionFactory.RunContainer()),
                new("todo-8", TaskDefinitionFactory.RunScript()),
                new("todo-9", TaskDefinitionFactory.RunShell()),
                new("todo-10", TaskDefinitionFactory.RunWorkflow()),
                new("todo-11", TaskDefinitionFactory.Switch()),
                new("todo-12", TaskDefinitionFactory.Try()),
                new("todo-13", TaskDefinitionFactory.Wait())
            ])
        };
    }

}
