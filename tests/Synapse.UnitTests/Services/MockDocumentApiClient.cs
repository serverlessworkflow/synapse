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

using Neuroglia.Data.Infrastructure.Services;
using Synapse.Api.Client.Services;

namespace Synapse.UnitTests.Services;

internal class MockDocumentApiClient(IRepository<Document, string> documents)
    : IDocumentApiClient
{

    public Task<Document> CreateAsync(string name, object? content, CancellationToken cancellationToken = default) => documents.AddAsync(new() { Name = name, Content = content! }, cancellationToken);

    public Task<Document> GetAsync(string id, CancellationToken cancellationToken = default) => documents.GetAsync(id, cancellationToken)!;

    public async Task UpdateAsync(string id, object content, CancellationToken cancellationToken = default)
    {
        var document = await documents.GetAsync(id, cancellationToken) ?? throw new NullReferenceException($"Failed to find a document with the specified id '{id}'");
        document.Content = content;
        await documents.UpdateAsync(document, cancellationToken);
    }

    public Task DeletesAsync(string id, CancellationToken cancellationToken = default) => documents.RemoveAsync(id, cancellationToken);

}