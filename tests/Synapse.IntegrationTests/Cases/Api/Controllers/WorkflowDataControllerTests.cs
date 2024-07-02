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
using System.Net.Http.Json;

namespace Synapse.IntegrationTests.Cases.Api.Controllers;

[TestCaseOrderer("Synapse.IntegrationTests.Services.PriorityTestCaseOrderer", "Synapse.IntegrationTests")]
public class WorkflowDataControllerTests(HttpApiFactory factory)
    : ControllerTestsBase(factory)
{

    protected override string Path => "/api/v1/workflow-data";

    [Fact, Priority(1)]
    public async Task Create_Data_Document_Should_Work()
    {
        //arrange
        var name = "fake-document-name";
        var content = new { foo = "bar", bar = new { baz = "foo" } };
        var document = new Document()
        {
            Name = name,
            Content = content
        };
        using var client = this.Factory.CreateClient();

        //act
        var response = await client.PostAsJsonAsync(this.Path, document);
        var result = await response.Content.ReadFromJsonAsync<Document>();

        //assert
        response.Should().BeSuccessful();
        //result.Should().BeEquivalentTo(document);
    }

    [Fact, Priority(2)]
    public async Task Get_Data_Document_Should_Work()
    {
        //arrange
        var name = "fake-document-name";
        var content = new { foo = "bar", bar = new { baz = "foo" } };
        var document = new Document()
        {
            Name = name,
            Content = content
        };
        using var client = this.Factory.CreateClient();
        var response = await client.PostAsJsonAsync(this.Path, document);
        document = (await response.Content.ReadFromJsonAsync<Document>())!;

        //act
        var result = await client.GetFromJsonAsync<Document>($"{this.Path}/{document.Id}");

        //assert
        //result.Should().BeEquivalentTo(document);
    }

}
