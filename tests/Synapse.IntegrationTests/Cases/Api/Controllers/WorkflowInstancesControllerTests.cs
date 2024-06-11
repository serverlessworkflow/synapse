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

using Synapse.IntegrationTests.Services;
using Synapse.Resources;
using FluentAssertions;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using System.Net.Http.Json;

namespace Synapse.IntegrationTests.Cases.Api.Controllers;

public class WorkflowInstancesControllerTests(HttpApiFactory factory)
    : ControllerTestsBase(factory)
{

    protected override string Path => "/api/v1/workflow-instances";

    [Fact, Priority(1)]
    public async Task Create_WorkflowInstance_Should_Work()
    {
        //arrange
        var definition = WorkflowDefinitionFactory.Create();
        var resource = WorkflowInstanceFactory.Create(definition);
        using var client = this.Factory.CreateClient();

        //act
        var response = await client.PostAsJsonAsync(this.Path, resource);

        //assert
        response.Should().BeSuccessful();
        (await response.Content.ReadFromJsonAsync<WorkflowInstance>()).Should().NotBeNull();
    }

    [Fact, Priority(2)]
    public async Task Get_WorkflowInstance_Should_Work()
    {
        //arrange
        var definition = WorkflowDefinitionFactory.Create();
        var resource = WorkflowInstanceFactory.Create(definition);
        using var client = this.Factory.CreateClient();
        resource = (await (await client.PostAsJsonAsync(this.Path, resource)).Content.ReadFromJsonAsync<WorkflowInstance>())!;

        //act
        var result = await client.GetFromJsonAsync<WorkflowInstance>($"{this.Path}/{resource.GetNamespace()}/{resource.GetName()}");

        //assert
        //result.Should().BeEquivalentTo(resource); //equivalency problem with dictionaries
    }

    [Fact, Priority(3)]
    public async Task List_WorkflowInstances_Should_Work()
    {
        //arrange
        var definition = WorkflowDefinitionFactory.Create();
        var resource = WorkflowInstanceFactory.Create(definition);
        using var client = this.Factory.CreateClient();
        resource = (await (await client.PostAsJsonAsync(this.Path, resource)).Content.ReadFromJsonAsync<WorkflowInstance>())!;

        //act
        var collection = await client.GetFromJsonAsync<Collection<WorkflowInstance>>(this.Path + $"/{resource.GetNamespace()}");

        //assert
        collection.Should().NotBeNull();
        collection!.Items.Should().HaveCount(1);
        collection.Items.Should().Contain(ns => ns.GetName() == resource.GetName());
    }

    [Fact, Priority(4)]
    public async Task Delete_WorkflowInstance_Should_Work()
    {
        //arrange
        var definition = WorkflowDefinitionFactory.Create();
        var resource = WorkflowInstanceFactory.Create(definition);
        using var client = this.Factory.CreateClient();
        resource = (await (await client.PostAsJsonAsync(this.Path, resource)).Content.ReadFromJsonAsync<WorkflowInstance>())!;

        //act
        var response = await client.DeleteAsync($"{this.Path}/{resource.GetNamespace()}/{resource.GetName()}");

        //assert
        response.Should().BeSuccessful();
        (await response.Content.ReadFromJsonAsync<WorkflowInstance>()).Should().NotBeNull();
    }

}
