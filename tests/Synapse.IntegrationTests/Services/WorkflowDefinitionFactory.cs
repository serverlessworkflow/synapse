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

using ServerlessWorkflow.Sdk.Builders;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.IntegrationTests.Services;

internal static class WorkflowDefinitionFactory
{

    internal static WorkflowDefinition Create()
    {
        return new WorkflowDefinitionBuilder()
            .WithName("fake")
            .WithVersion("1.0.0")
            .Do("todo-1", task => task
                .Call("http")
                    .With("method", HttpMethod.Get.Method)
                    .With("uri", new Uri("https://petstore.swagger.io/v2/pet/1")))
            .Build();
    }

}
