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

namespace Synapse.Runner.Services;

/// <summary>
/// Represents an in-memory implementation of the <see cref="IDocumentApiClient"/> interface
/// </summary>
public class MemoryDocumentManager
    : IDocumentApiClient
{

    /// <summary>
    /// Gets a key/value mapping of all documents stored in memory
    /// </summary>
    protected IDictionary<string, Document> Documents { get; } = new Dictionary<string, Document>();

    /// <inheritdoc/>
    public virtual Task<Document> CreateAsync(string name, object content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(content);
        var document = new Document()
        {
            Name = name,
            Content = content
        };
        this.Documents[document.Id] = document;
        return Task.FromResult(document);
    }

    /// <inheritdoc/>
    public virtual Task<Document> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Task.FromResult(this.Documents[id]);
    }

    /// <inheritdoc/>
    public virtual Task UpdateAsync(string id, object content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(content);
        var document = this.Documents[id];
        document.Content = content;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        this.Documents.Remove(id);
        return Task.CompletedTask;
    }

}