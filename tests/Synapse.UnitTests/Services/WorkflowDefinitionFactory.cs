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
