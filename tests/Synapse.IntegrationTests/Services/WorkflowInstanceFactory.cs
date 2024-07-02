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

using Synapse.Resources;
using ServerlessWorkflow.Sdk.Models;
using Neuroglia;

namespace Synapse.IntegrationTests.Services;

internal static class WorkflowInstanceFactory
{

    internal static WorkflowInstance Create(WorkflowDefinition definition)
    {
        return new(
            new() 
            { 
                Name = definition.Document.Name, 
                Namespace = definition.Document.Namespace
            }, 
            new()
            { 
                Definition = $"{definition.Document.Namespace}.{definition.Document.Name}:{definition.Document.Version}",
                Input = new EquatableDictionary<string, object>()
                {
                    { "foo", "bar" },
                }
            });
    }

}
