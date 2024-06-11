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