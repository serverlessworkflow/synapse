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

using Synapse.Runner.Services;

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