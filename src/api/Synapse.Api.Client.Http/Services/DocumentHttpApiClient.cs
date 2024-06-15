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

namespace Synapse.Api.Client.Services;

/// <summary>
/// Represents the default HTTP implementation of the <see cref="IDocumentApiClient"/> interface
/// </summary>
/// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
/// <param name="logger">The service used to perform logging</param>
/// <param name="jsonSerializer">The service used to serialize/deserialize objects to/from JSON</param>
/// <param name="options">The service used to access the current <see cref="SynapseHttpApiClientOptions"/></param>
/// <param name="httpClient">The service used to perform HTTP requests</param>
public class DocumentHttpApiClient(IServiceProvider serviceProvider, ILogger<DocumentHttpApiClient> logger, IJsonSerializer jsonSerializer, IOptions<SynapseHttpApiClientOptions> options, HttpClient httpClient)
    : ApiClientBase(serviceProvider, logger, jsonSerializer, options, httpClient), IDocumentApiClient
{

    /// <inheritdoc/>
    public virtual async Task<Document> CreateAsync(string documentName, object documentContent, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentName);
        var document = new Document() 
        { 
            Name = documentName, 
            Content = documentContent 
        };
        var json =  this.JsonSerializer.SerializeToText(document);
        using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        var uri = "api/v1/workflow-data";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Post, uri) { Content = content }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<Document>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task<Document> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var uri = $"api/v1/workflow-data/{id}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return this.JsonSerializer.Deserialize<Document>(json)!;
    }

    /// <inheritdoc/>
    public virtual async Task UpdateAsync(string id, object content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var json = this.JsonSerializer.SerializeToText(content);
        using var requestContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        var uri = $"api/v1/workflow-data/{id}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Put, uri) { Content = requestContent }, cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeletesAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var uri = $"api/v1/workflow-data/{id}";
        using var request = await this.ProcessRequestAsync(new HttpRequestMessage(HttpMethod.Delete, uri), cancellationToken).ConfigureAwait(false);
        using var response = await this.ProcessResponseAsync(await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

}
