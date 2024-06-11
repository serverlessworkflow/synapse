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
using FluentAssertions;
using Neuroglia.Data.Infrastructure.ResourceOriented;
using System.Net.Http.Json;

namespace Synapse.IntegrationTests.Cases.Api.Controllers;

[TestCaseOrderer("Synapse.IntegrationTests.Services.PriorityTestCaseOrderer", "Synapse.IntegrationTests")]
public class NamespacesControllerTests(HttpApiFactory factory)
    : ControllerTestsBase(factory)
{

    protected override string Path => "/api/v1/namespaces";

    [Fact, Priority(1)]
    public async Task Create_Namespace_Should_Work()
    {
        //arrange
        var resource = new Namespace("fake-namespace");
        using var client = this.Factory.CreateClient();

        //act
        var response = await client.PostAsJsonAsync(this.Path, resource);

        //assert
        response.Should().BeSuccessful();
        (await response.Content.ReadFromJsonAsync<Namespace>()).Should().NotBeNull();
    }

    [Fact, Priority(2)]
    public async Task Get_Namespace_Should_Work()
    {
        //arrange
        var resource = new Namespace("fake-namespace");
        using var client = this.Factory.CreateClient();
        resource = (await (await client.PostAsJsonAsync(this.Path, resource)).Content.ReadFromJsonAsync<Namespace>())!;

        //act
        var result = await client.GetFromJsonAsync<Namespace>($"{this.Path}/{resource.GetName()}");

        //assert
        result.Should().BeEquivalentTo(resource);
    }

    [Fact, Priority(3)]
    public async Task List_Namespaces_Should_Work()
    {
        //arrange
        var resource = new Namespace("fake-namespace");
        using var client = this.Factory.CreateClient();
        resource = (await (await client.PostAsJsonAsync(this.Path, resource)).Content.ReadFromJsonAsync<Namespace>())!;

        //act
        var collection = await client.GetFromJsonAsync<Collection<Namespace>>(this.Path);

        //assert
        collection.Should().NotBeNull();
        collection!.Items.Should().HaveCount(2);
        collection.Items.Should().Contain(ns => ns.GetName() == resource.GetName());
    }

    [Fact, Priority(4)]
    public async Task Delete_Namespace_Should_Work()
    {
        //arrange
        var resource = new Namespace("fake-namespace");
        using var client = this.Factory.CreateClient();
        resource = (await (await client.PostAsJsonAsync(this.Path, resource)).Content.ReadFromJsonAsync<Namespace>())!;

        //act
        var response = await client.DeleteAsync($"{this.Path}/{resource.GetName()}");

        //assert
        response.Should().BeSuccessful();
        (await response.Content.ReadFromJsonAsync<Namespace>()).Should().NotBeNull();
    }

}
